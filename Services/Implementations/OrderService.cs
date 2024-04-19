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

    public async Task<ServiceResponse<List<Order>>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingInfo)  // Include shipping details
                .ToListAsync();
            return new ServiceResponse<List<Order>>
            {
                Data = orders,
                Message = "Retrieved all orders successfully."
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

    public async Task<ServiceResponse<Order>> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingInfo)  // Load shipping details
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
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                response.Success = false;
                response.Message = "Order not found.";
                return response;
            }

            order.Status = updatedOrder.Status;
            order.UpdatedAt = DateTime.UtcNow;
            order.ShippingInfo.ShippingCost = updatedOrder.ShippingInfo.ShippingCost; // Updating shipping cost

            decimal totalPrice = 0;
            foreach (var updatedItem in updatedOrder.OrderItems)
            {
                var existingItem = order.OrderItems.FirstOrDefault(oi => oi.Id == updatedItem.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity = updatedItem.Quantity;
                    existingItem.Price = updatedItem.Price; // Assuming price per unit might have changed
                    totalPrice += existingItem.Price * existingItem.Quantity;
                }
                else
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = updatedItem.ProductId,
                        Quantity = updatedItem.Quantity,
                        Price = updatedItem.Price
                    });
                    totalPrice += updatedItem.Price * updatedItem.Quantity;
                }
            }

            // Include the shipping cost in the total price
            totalPrice += order.ShippingInfo.ShippingCost;

            order.TotalPrice = totalPrice;

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
