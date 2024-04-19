using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

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
