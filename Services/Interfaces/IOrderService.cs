using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IOrderService
{
    Task<ServiceResponse<List<Order>>> GetAllOrdersAsync(
        int? userId,
        string? status,
        decimal? minTotalPrice,
        decimal? maxTotalPrice,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? updatedFrom,
        DateTime? updatedTo
    );
    Task<ServiceResponse<Order>> GetOrderByIdAsync(int orderId);
    Task<ServiceResponse<Order>> CreateOrderAsync(Order order);
    Task<ServiceResponse<Order>> UpdateOrderAsync(int orderId, Order order);
    Task<ServiceResponse<bool>> DeleteOrderAsync(int orderId);
}
