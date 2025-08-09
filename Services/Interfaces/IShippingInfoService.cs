using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IShippingInfoService
{
    Task<ServiceResponse<ShippingInfo>> CreateShippingInfoAsync(ShippingInfo shippingInfo);
    Task<ServiceResponse<List<ShippingInfo>>> GetAllShippingInfosAsync();
    Task<ServiceResponse<ShippingInfo>> GetShippingInfoByIdAsync(int id);
    Task<ServiceResponse<ShippingInfo>> UpdateShippingInfoAsync(int id, ShippingInfo updated);
    Task<ServiceResponse<bool>> DeleteShippingInfoAsync(int id);
    Task<ServiceResponse<ShippingInfo>> GetShippingInfoByOrderIdAsync(int orderId);
}


