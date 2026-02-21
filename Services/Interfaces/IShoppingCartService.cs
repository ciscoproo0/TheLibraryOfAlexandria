using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IShoppingCartService defines the contract for shopping cart management operations.
/// This service handles cart creation, item management, and cart lifecycle for customer purchases.
/// Each user has a single active shopping cart containing multiple items before checkout.
/// </summary>
public interface IShoppingCartService
{
    /// <summary>
    /// Creates a new empty shopping cart for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user owning the cart.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created ShoppingCart object with empty Items list
    /// - Failure: Error message if cart creation fails (e.g., user not found, cart already exists)
    /// </returns>
    /// <remarks>
    /// Each user should have exactly one active shopping cart at a time.
    /// Cart is initially empty and items are added via AddItemToCartAsync.
    /// CreatedAt timestamp is automatically set to current UTC time.
    /// </remarks>
    Task<ServiceResponse<ShoppingCart>> CreateCartForUserAsync(int userId);

    /// <summary>
    /// Retrieves a user's shopping cart with all items.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cart to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: ShoppingCart object with all items populated
    /// - Failure: Error message if cart not found for user or user doesn't exist
    /// </returns>
    Task<ServiceResponse<ShoppingCart>> GetCartByUserIdAsync(int userId);

    /// <summary>
    /// Adds an item (product) to a shopping cart or updates quantity if product already exists in cart.
    /// </summary>
    /// <param name="cartId">The unique identifier of the shopping cart.</param>
    /// <param name="item">The ShoppingCartItem containing product ID and desired quantity.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Added/updated ShoppingCartItem object
    /// - Failure: Error message if item add fails (e.g., cart not found, product out of stock)
    /// </returns>
    /// <remarks>
    /// If product already exists in cart, quantity is updated to the new value.
    /// Stock availability is validated but not decremented until checkout.
    /// Item price is captured at add-time for consistent pricing.
    /// </remarks>
    Task<ServiceResponse<ShoppingCartItem>> AddItemToCartAsync(int cartId, ShoppingCartItem item);

    /// <summary>
    /// Removes a specific item from a shopping cart.
    /// </summary>
    /// <param name="itemId">The unique identifier of the shopping cart item to remove.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if removal succeeded
    /// - Failure: Error message if item not found or removal fails
    /// </returns>
    Task<ServiceResponse<bool>> RemoveItemFromCartAsync(int itemId);

    /// <summary>
    /// Clears all items from a shopping cart, leaving it empty.
    /// </summary>
    /// <param name="cartId">The unique identifier of the shopping cart to clear.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if cart was cleared successfully
    /// - Failure: Error message if cart not found or clear fails
    /// </returns>
    /// <remarks>
    /// This is typically called after successful order checkout or by user request to abandon cart.
    /// Cart itself remains in database (not deleted), only items are removed.
    /// </remarks>
    Task<ServiceResponse<bool>> ClearCartAsync(int cartId);
}