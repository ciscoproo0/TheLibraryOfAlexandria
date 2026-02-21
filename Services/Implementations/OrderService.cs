using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// OrderService implements order management operations including CRUD functionality and advanced filtering.
/// This service handles order creation with stock management, retrieval with multiple filter criteria,
/// status updates with business rule validation, and cascading deletion.
/// All database operations include comprehensive error handling and transaction support.
/// </summary>
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of OrderService with database context.
    /// </summary>
    /// <param name="context">Entity Framework database context for order persistence and querying.</param>
    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all orders with optional multi-criteria filtering.
    /// Supports filtering by user, status, price range, and creation/update dates.
    /// Results are ordered by creation date (newest first).
    /// Includes all related OrderItems and ShippingInfo data via eager loading.
    /// </summary>
    public async Task<ServiceResponse<List<Order>>> GetAllOrdersAsync(
        int? userId,
        string? status,
        decimal? minTotalPrice,
        decimal? maxTotalPrice,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? updatedFrom,
        DateTime? updatedTo
    )
    {
        try
        {
            // Start with base query, eagerly load related entities to avoid N+1 queries
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingInfo)
                .AsQueryable();

            // Apply optional filters - all use AND logic
            if (userId.HasValue)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }
            if (minTotalPrice.HasValue)
            {
                query = query.Where(o => o.TotalPrice >= minTotalPrice.Value);
            }
            if (maxTotalPrice.HasValue)
            {
                query = query.Where(o => o.TotalPrice <= maxTotalPrice.Value);
            }
            if (createdFrom.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= createdFrom.Value);
            }
            if (createdTo.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= createdTo.Value);
            }
            if (updatedFrom.HasValue)
            {
                query = query.Where(o => o.UpdatedAt >= updatedFrom.Value);
            }
            if (updatedTo.HasValue)
            {
                query = query.Where(o => o.UpdatedAt <= updatedTo.Value);
            }

            // Sort by most recent first
            query = query.OrderByDescending(o => o.CreatedAt);

            var items = await query.ToListAsync();

            return new ServiceResponse<List<Order>>
            {
                Data = items,
                Message = "Retrieved orders successfully."
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<Order>>
            {
                Success = false,
                Message = $"An error occurred while retrieving orders: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Retrieves a single order by ID with all related OrderItems and ShippingInfo.
    /// </summary>
    public async Task<ServiceResponse<Order>> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return new ServiceResponse<Order>
                {
                    Success = false,
                    Message = "Order not found."
                };
            }
            return new ServiceResponse<Order>
            {
                Data = order,
                Message = "Order retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Order>
            {
                Success = false,
                Message = $"An error occurred while retrieving the order: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new order with validation, stock management, and relational setup.
    /// For each OrderItem: validates product existence and stock, decrements inventory, captures price.
    /// Sets up ShippingInfo FK relationship and calculates total including shipping cost.
    /// All changes are transactional - failure at any step rolls back the entire order.
    /// </summary>
    public async Task<ServiceResponse<Order>> CreateOrderAsync(Order order)
    {
        var serviceResponse = new ServiceResponse<Order>();
        try
        {
            decimal totalPrice = 0;

            // Process each order line: validate stock and update inventory
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Product with ID {item.ProductId} is out of stock or does not exist.";
                    return serviceResponse;
                }

                // Capture price at order time for historical accuracy
                item.Price = product.Price;
                totalPrice += item.Price * item.Quantity;

                // Decrement product stock
                product.StockQuantity -= item.Quantity;
            }

            // Ensure shipping info is linked correctly to the order being created
            if (order.ShippingInfo != null)
            {
                order.ShippingInfo.OrderId = order.Id; // Ensures FK constraint is satisfied
                // Add shipping cost to total price (prevents null reference exception)
                totalPrice += order.ShippingInfo.ShippingCost;
            }

            // Set computed total price
            order.TotalPrice = totalPrice;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            serviceResponse.Data = order;
            serviceResponse.Message = "Order created successfully with shipping info.";
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = $"An error occurred while creating the order: {ex.Message}";
        }

        return serviceResponse;
    }

    /// <summary>
    /// Updates an existing order with business rule validation for status transitions.
    /// When transitioning to "completed", validates that payment is Completed and shipping is Delivered.
    /// Only order-level properties are updated here (Status); items/shipping use dedicated endpoints.
    /// Automatically updates UpdatedAt timestamp on successful save.
    /// </summary>
    public async Task<ServiceResponse<Order>> UpdateOrderAsync(int orderId, Order updatedOrder)
    {
        var response = new ServiceResponse<Order>();
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingInfo)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                response.Success = false;
                response.Message = "Order not found.";
                return response;
            }

            // Validate status transition to "completed" requires payment completed + shipping delivered
            var requestedStatus = updatedOrder.Status?.Trim();
            if (!string.IsNullOrWhiteSpace(requestedStatus) && requestedStatus.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                // Use AsNoTracking to avoid stale entity issues in concurrent scenarios
                var paymentForOrder = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == order.Id);
                var latestShippingStatus = await _context.ShippingInfos.AsNoTracking()
                    .Where(s => s.OrderId == order.Id)
                    .Select(s => (ShippingStatus?)s.Status)
                    .FirstOrDefaultAsync();

                var paymentState = paymentForOrder?.Status.ToString() ?? "None";
                var shippingState = latestShippingStatus?.ToString() ?? "None";

                // Require both payment completion and delivery before marking order "completed"
                bool isPaymentCompleted = paymentForOrder != null && paymentForOrder.Status == PaymentStatus.Completed;
                bool isShippingDelivered = latestShippingStatus.HasValue && latestShippingStatus.Value == ShippingStatus.Delivered;
                if (!(isPaymentCompleted && isShippingDelivered))
                {
                    response.Success = false;
                    response.Message = $"Cannot set order status to 'completed'. Requirements: payment must be 'Completed' and shipping must be 'Delivered'. Current states -> payment: '{paymentState}', shipping: '{shippingState}'.";
                    return response;
                }
            }

            // Apply order-level updates only (items/shipping managed via their services)
            if (!string.IsNullOrWhiteSpace(updatedOrder.Status))
            {
                order.Status = updatedOrder.Status;
            }
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            response.Data = order;
            response.Message = "Order updated successfully.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"An error occurred while updating the order: {ex.Message}";
        }

        return response;
    }

    /// <summary>
    /// Deletes an order and all related entities (OrderItems, Payment, ShippingInfo via cascade FK).
    /// </summary>
    public async Task<ServiceResponse<bool>> DeleteOrderAsync(int id)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return new ServiceResponse<bool> { Success = false, Message = "Order not found." };
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true, Message = "Order deleted successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = $"An error occurred while deleting the order: {ex.Message}" };
        }
    }
}
