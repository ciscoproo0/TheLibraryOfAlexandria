using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IOrderService
{
    Task<ServiceResponse<List<Order>>> GetAllOrdersAsync();
    Task<ServiceResponse<Order>> GetOrderByIdAsync(int orderId);
    Task<ServiceResponse<Order>> CreateOrderAsync(Order order);
    Task<ServiceResponse<Order>> UpdateOrderAsync(int orderId, Order order);
    Task<ServiceResponse<bool>> DeleteOrderAsync(int orderId);
}
