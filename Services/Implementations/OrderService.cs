using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<PaginatedResult<Order>>> GetAllOrdersAsync(
        int page,
        int pageSize,
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
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingInfo)
                .AsQueryable();

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

            query = query.OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ServiceResponse<PaginatedResult<Order>>
            {
                Data = new PaginatedResult<Order>
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                },
                Message = "Retrieved orders page successfully."
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<PaginatedResult<Order>>
            {
                Success = false,
                Message = $"An error occurred while retrieving orders: {ex.Message}"
            };
        }
    }

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

    public async Task<ServiceResponse<Order>> CreateOrderAsync(Order order)
    {
        var serviceResponse = new ServiceResponse<Order>();
        try
        {
            decimal totalPrice = 0;

            // Process each line of the order
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = $"Product with ID {item.ProductId} is out of stock or does not exist.";
                    return serviceResponse;
                }

                item.Price = product.Price;
                totalPrice += item.Price * item.Quantity;

                // Update stock
                product.StockQuantity -= item.Quantity;
            }

            // Ensure shipping info is linked correctly to the order being created
            if (order.ShippingInfo != null)
            {
                order.ShippingInfo.OrderId = order.Id; // This ensures the FK constraint is satisfied
            }

            totalPrice += order.ShippingInfo.ShippingCost;

            order.TotalPrice = totalPrice; // Set total price

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

            // Requested status change (Order-level only). We do not update items or shipping here.
            var requestedStatus = updatedOrder.Status?.Trim();
            if (!string.IsNullOrWhiteSpace(requestedStatus) && requestedStatus.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                // Validate: to set order as completed, payment must be Completed and shipping Delivered
                var paymentForOrder = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
                var paymentState = paymentForOrder?.Status.ToString() ?? "None";
                var targetShippingStatus = updatedOrder.ShippingInfo?.Status ?? order.ShippingInfo?.Status;
                var shippingState = targetShippingStatus?.ToString() ?? "None";

                bool isPaymentCompleted = paymentForOrder != null && paymentForOrder.Status == PaymentStatus.Completed;
                bool isShippingDelivered = targetShippingStatus.HasValue && targetShippingStatus.Value == ShippingStatus.Delivered;
                if (!(isPaymentCompleted && isShippingDelivered))
                {
                    response.Success = false;
                    response.Message = $"Cannot set order status to 'completed'. Requirements: payment must be 'Completed' and shipping must be 'Delivered'. Current states -> payment: '{paymentState}', shipping: '{shippingState}'.";
                    return response;
                }
            }

            // Apply order-level updates only
            if (!string.IsNullOrWhiteSpace(updatedOrder.Status))
            {
                order.Status = updatedOrder.Status;
            }
            order.UpdatedAt = DateTime.UtcNow;
            // Note: TotalPrice recomputation and Shipping/Items updates are handled by their dedicated endpoints/services.

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
