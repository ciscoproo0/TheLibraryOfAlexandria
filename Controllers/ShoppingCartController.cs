using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// ShoppingCartController manages shopping cart lifecycle for customers.
/// Provides endpoints for creating carts, adding/removing items, and clearing cart contents.
/// Customers maintain a single active shopping cart per user; duplicate carts are prevented.
/// Stock inventory is validated but not decremented until order checkout/creation.
/// Route: api/ShoppingCart
/// </summary>
[Authorize(Roles = "Customer, Admin, SuperAdmin")]
[Route("api/[controller]")]
[ApiController]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    /// <summary>
    /// Creates a new shopping cart for a user.
    /// Each user can have only one active shopping cart; attempt to create duplicate cart returns error.
    /// Cart starts empty and items can be added via AddItemToCart endpoint.
    /// </summary>
    /// <param name="userId">User ID for which to create shopping cart</param>
    /// <returns>Created shopping cart with generated ID and empty items list</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins can create carts.
    /// Business Rules:
    /// - Only one active cart per user is allowed
    /// - If cart already exists for user, returns error message instead of creating duplicate
    /// - New carts start with empty ShoppingCartItems collection
    /// Status Codes:
    /// - 201 Created: Cart created successfully, Location header contains GET endpoint
    /// - 400 BadRequest: User does not exist or cart already exists for this user
    /// </remarks>
    [HttpPost("create/{userId}")]
    public async Task<ActionResult<ShoppingCart>> CreateCartForUser(int userId)
    {
        var response = await _shoppingCartService.CreateCartForUserAsync(userId);
        if (!response.Success)
        {
            return BadRequest(response.Message);
        }
        return CreatedAtAction(nameof(GetCartByUserId), new { userId = response?.Data?.UserId }, response?.Data);
    }

    /// <summary>
    /// Retrieves the shopping cart for a specific user.
    /// Returns cart details including all ShoppingCartItems currently in the cart.
    /// </summary>
    /// <param name="userId">User ID for which to retrieve shopping cart</param>
    /// <returns>ShoppingCart object with id, userId, and ShoppingCartItems collection</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins.
    /// Status Codes:
    /// - 200 OK: Cart found
    /// - 404 NotFound: No cart exists for this user (user must create cart first)
    /// </remarks>
    // GET: api/ShoppingCart/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ServiceResponse<ShoppingCart>>> GetCartByUserId(int userId)
    {
        var serviceResponse = await _shoppingCartService.GetCartByUserIdAsync(userId);
        if (!serviceResponse.Success)
        {
            return NotFound(serviceResponse.Message);
        }
        return Ok(serviceResponse);
    }

    /// <summary>
    /// Adds a product item to the shopping cart with specified quantity.
    /// Stock inventory is validated; product must have sufficient quantity available.
    /// Adding duplicate product to cart increases quantity (overwrites previous quantity).
    /// Note: Inventory is NOT decremented; only validated for availability.
    /// Stock is actually decremented when order is created/checked out.
    /// </summary>
    /// <param name="cartId">Shopping cart ID to add item to</param>
    /// <param name="itemDto">ShoppingCartItem with ProductId, Quantity, and Price (unit price per item)</param>
    /// <returns>Added ShoppingCartItem with generated ID</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins.
    /// Business Rules:
    /// - Product must exist and have stock quantity >= requested quantity
    /// - Price field should reflect current unit price of product at time of add
    /// - Adding same product twice overwrites (replaces) previous quantity
    /// - Stock is NOT decremented from inventory (only validated for sufficiency)
    /// - Inventory decrement happens when order is created via OrdersController
    /// Status Codes:
    /// - 201 Created: Item added successfully, Location header contains GetCartByUserId endpoint
    /// - 400 BadRequest: Product out of stock, product not found, or quantity invalid
    /// - 404 NotFound: Cart not found
    /// </remarks>
    // POST: api/ShoppingCart/5/items
    [HttpPost("{cartId}/items")]
    public async Task<ActionResult<ServiceResponse<ShoppingCartItem>>> AddItemToCart(int cartId, [FromBody] ShoppingCartItem itemDto)
    {
        var serviceResponse = await _shoppingCartService.AddItemToCartAsync(cartId, itemDto);
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.Message);
        }
        return CreatedAtAction("GetCartByUserId", new { userId = cartId }, serviceResponse.Data);
    }

    /// <summary>
    /// Removes a single item from the shopping cart by ShoppingCartItem ID.
    /// Removing item completely deletes the line item; to reduce quantity, update the item or re-add with lower quantity.
    /// </summary>
    /// <param name="itemId">ShoppingCartItem ID to remove</param>
    /// <returns>204 NoContent on success</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins.
    /// Status Codes:
    /// - 204 NoContent: Item removed successfully
    /// - 404 NotFound: ShoppingCartItem does not exist
    /// </remarks>
    // DELETE: api/ShoppingCart/items/5
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveItemFromCart(int itemId)
    {
        var serviceResponse = await _shoppingCartService.RemoveItemFromCartAsync(itemId);
        if (!serviceResponse.Success)
        {
            return NotFound(serviceResponse.Message);
        }
        return NoContent();
    }

    /// <summary>
    /// Clears all items from a shopping cart, leaving the cart empty but intact.
    /// Cart itself is not deleted; only all ShoppingCartItems are removed.
    /// </summary>
    /// <param name="cartId">Shopping cart ID to clear</param>
    /// <returns>204 NoContent on success</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins.
    /// Status Codes:
    /// - 204 NoContent: Cart cleared successfully (now empty)
    /// - 400 BadRequest: Cart not found or service error
    /// </remarks>
    // POST: api/ShoppingCart/5/clear
    [HttpPost("{cartId}/clear")]
    public async Task<IActionResult> ClearCart(int cartId)
    {
        var serviceResponse = await _shoppingCartService.ClearCartAsync(cartId);
        if (!serviceResponse.Success)
        {
            return BadRequest(serviceResponse.Message);
        }
        return NoContent();
    }
}
