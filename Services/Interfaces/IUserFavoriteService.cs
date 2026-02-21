using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IUserFavoriteService defines the contract for user favorites (wishlist) management operations.
/// This service handles storing, retrieving, and managing favorite Magic: The Gathering products for users.
/// Favorites allow customers to save products for later purchase consideration.
/// </summary>
public interface IUserFavoriteService
{
    /// <summary>
    /// Retrieves all favorite products for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose favorites to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of UserFavorite objects containing product information for the user
    /// - Failure: Error message if user not found or query fails
    /// </returns>
    /// <remarks>
    /// Returns empty list if user has no favorites (not an error condition).
    /// Each UserFavorite contains linked Product details for display in UI.
    /// </remarks>
    Task<ServiceResponse<List<UserFavorite>>> GetUserFavoritesAsync(int userId);

    /// <summary>
    /// Retrieves a single favorite by its ID.
    /// </summary>
    /// <param name="favoriteId">The unique identifier of the favorite to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Complete UserFavorite object with product details
    /// - Failure: Error message if favorite not found
    /// </returns>
    Task<ServiceResponse<UserFavorite>> GetFavoriteByIdAsync(int favoriteId);

    /// <summary>
    /// Adds a product to a user's favorites list.
    /// </summary>
    /// <param name="favorite">The UserFavorite object containing user ID and product ID to favorite.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created UserFavorite object
    /// - Failure: Error message if addition fails (e.g., user/product not found, already favorited)
    /// </returns>
    /// <remarks>
    /// Prevents duplicate favorites (same user-product pair cannot be favorited twice).
    /// Timestamp is automatically recorded when favorite is added.
    /// </remarks>
    Task<ServiceResponse<UserFavorite>> AddFavoriteAsync(UserFavorite favorite);

    /// <summary>
    /// Removes a product from a user's favorites list.
    /// </summary>
    /// <param name="favoriteId">The unique identifier of the favorite to remove.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if removal succeeded
    /// - Failure: Error message if favorite not found or removal fails
    /// </returns>
    Task<ServiceResponse<bool>> RemoveFavoriteAsync(int favoriteId);
}
