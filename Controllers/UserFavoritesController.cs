using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// UserFavoriteController manages user favorites (wishlist) functionality.
/// Allows customers to save products for later consideration without affecting inventory.
/// Favorites are user-specific collections of products with duplicate prevention.
/// Route: api/UserFavorite
/// </summary>
[Authorize(Roles = "Customer, Admin, SuperAdmin")]
[Route("api/[controller]")]
[ApiController]
public class UserFavoriteController : ControllerBase
{
    private readonly IUserFavoriteService _userFavoriteService;

    public UserFavoriteController(IUserFavoriteService userFavoriteService)
    {
        _userFavoriteService = userFavoriteService;
    }

    /// <summary>
    /// Retrieves all products in a user's favorites list (wishlist).
    /// Includes related product and user information for complete favorite details.
    /// </summary>
    /// <param name="userId">User ID for which to retrieve favorites</param>
    /// <returns>List of UserFavorite items with related Product and User information</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins.
    /// Status Codes:
    /// - 200 OK: Favorites retrieved successfully (may be empty list)
    /// - 404 NotFound: User not found or no favorites exist yet
    /// </remarks>
    [HttpGet("{userId}")]
    public async Task<ActionResult<ServiceResponse<List<UserFavorite>>>> GetUserFavorites(int userId)
    {
        var response = await _userFavoriteService.GetUserFavoritesAsync(userId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a single favorite (UserFavorite record) by ID.
    /// Returns the specific favorite entry with product and user details.
    /// </summary>
    /// <param name="favoriteId">UserFavorite ID to retrieve</param>
    /// <returns>UserFavorite record with related Product and User information</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins.
    /// Status Codes:
    /// - 200 OK: Favorite found
    /// - 404 NotFound: Favorite does not exist
    /// </remarks>
    [HttpGet("single/{favoriteId}")]
    public async Task<ActionResult<ServiceResponse<UserFavorite>>> GetFavoriteById(int favoriteId)
    {
        var response = await _userFavoriteService.GetFavoriteByIdAsync(favoriteId);
        if (!response.Success)
            return NotFound(response.Message);
        return Ok(response);
    }

    /// <summary>
    /// Adds a product to a user's favorites list (wishlist).
    /// Prevents duplicate entries; adding the same product twice returns error or overwrites (implementation-dependent).
    /// Does not affect product inventory or shopping cart.
    /// </summary>
    /// <param name="favorite">UserFavorite with userId and productId to add to favorites</param>
    /// <returns>Created UserFavorite record with generated ID and timestamp</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins can add favorites.
    /// Business Rules:
    /// - User and Product must exist (FK validation)
    /// - Duplicate prevention: adding same product twice to same user returns error
    /// - Does not affect shopping cart or inventory
    /// - Favorites serve as wishlist/save-for-later functionality
    /// Status Codes:
    /// - 201 Created: Favorite added successfully, Location header contains GET endpoint
    /// - 400 BadRequest: Duplicate favorite, product/user not found, or service error
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<UserFavorite>>> AddFavorite([FromBody] UserFavorite favorite)
    {
        var response = await _userFavoriteService.AddFavoriteAsync(favorite);
        if (!response.Success)
            return BadRequest(response.Message);
        return CreatedAtAction(nameof(GetFavoriteById), new { favoriteId = response?.Data?.Id }, response?.Data);
    }

    /// <summary>
    /// Removes a product from a user's favorites list by favorite ID.
    /// Deletes the UserFavorite record, permanently removing the product from user's wishlist.
    /// </summary>
    /// <param name="favoriteId">UserFavorite ID to remove</param>
    /// <returns>204 NoContent on success</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins can remove favorites.
    /// Status Codes:
    /// - 204 NoContent: Favorite removed successfully
    /// - 404 NotFound: Favorite does not exist
    /// </remarks>
    [HttpDelete("{favoriteId}")]
    public async Task<ActionResult<ServiceResponse<bool>>> RemoveFavorite(int favoriteId)
    {
        var response = await _userFavoriteService.RemoveFavoriteAsync(favoriteId);
        if (!response.Success)
            return NotFound(response.Message);
        return NoContent();
    }
}
