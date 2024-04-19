using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IShoppingCartService
{
    Task<ServiceResponse<ShoppingCart>> CreateCartForUserAsync(int userId);
    Task<ServiceResponse<ShoppingCart>> GetCartByUserIdAsync(int userId);
    Task<ServiceResponse<ShoppingCartItem>> AddItemToCartAsync(int cartId, ShoppingCartItem item);
    Task<ServiceResponse<bool>> RemoveItemFromCartAsync(int itemId);
    Task<ServiceResponse<bool>> ClearCartAsync(int cartId);
}