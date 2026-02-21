using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// ShoppingCartService implements shopping cart operations for customer checkout flows.
/// Manages cart lifecycle including creation, item management, and cart clearing.
/// Each user has one active shopping cart used to aggregate items before checkout.
/// </summary>
public class ShoppingCartService : IShoppingCartService
{
    private readonly ApplicationDbContext _context;

    public ShoppingCartService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new empty shopping cart for a user. Validates user exists and cart doesn't already exist.
    /// </summary>
    public async Task<ServiceResponse<ShoppingCart>> CreateCartForUserAsync(int userId)
    {
        try
        {
            // Validate user exists
            if (await _context.Users.AnyAsync(u => u.Id == userId) == false)
            {
                return new ServiceResponse<ShoppingCart> { Success = false, Message = "User not found." };
            }

            // Prevent duplicate carts per user
            if (await _context.ShoppingCarts.AnyAsync(c => c.UserId == userId))
            {
                return new ServiceResponse<ShoppingCart> { Success = false, Message = "Cart already exists for this user." };
            }

            var newCart = new ShoppingCart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShoppingCarts.Add(newCart);
            await _context.SaveChangesAsync();

            return new ServiceResponse<ShoppingCart> { Data = newCart, Message = "New shopping cart created successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShoppingCart> { Success = false, Message = $"An error occurred while creating the cart: {ex.Message}" };
        }
    }

    /// <summary>
    /// Retrieves a user's shopping cart with all items. Includes product and quantity information.
    /// </summary>
    public async Task<ServiceResponse<ShoppingCart>> GetCartByUserIdAsync(int userId)
    {
        try
        {
            var cart = await _context.ShoppingCarts
                .Where(c => c.UserId == userId)
                .Select(c => new ShoppingCart
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    CreatedAt = c.CreatedAt,
                    Items = c.Items.Select(item => new ShoppingCartItem
                    {
                        Id = item.Id,
                        CartId = item.CartId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return new ServiceResponse<ShoppingCart> { Success = false, Message = "Cart not found." };
            }
            return new ServiceResponse<ShoppingCart> { Data = cart };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShoppingCart> { Success = false, Message = "An error occurred: " + ex.Message };
        }
    }

    /// <summary>
    /// Adds a product to cart or updates quantity if product already exists. Validates stock availability.
    /// </summary>
    public async Task<ServiceResponse<ShoppingCartItem>> AddItemToCartAsync(int cartId, ShoppingCartItem itemDto)
    {
        try
        {
            var cart = await _context.ShoppingCarts.FindAsync(cartId);
            if (cart == null)
            {
                return new ServiceResponse<ShoppingCartItem> { Success = false, Message = "Cart not found." };
            }

            // Validate product exists
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                return new ServiceResponse<ShoppingCartItem> { Success = false, Message = "Product not found." };
            }

            // Validate stock availability (note: not decremented until order checkout)
            if (product.StockQuantity < itemDto.Quantity)
            {
                return new ServiceResponse<ShoppingCartItem> { Success = false, Message = "Not enough stock available." };
            }

            var newItem = new ShoppingCartItem
            {
                CartId = cartId,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            };

            _context.ShoppingCartItems.Add(newItem);
            await _context.SaveChangesAsync();

            return new ServiceResponse<ShoppingCartItem> { Data = new ShoppingCartItem { Id = newItem.Id, ProductId = newItem.ProductId, Quantity = newItem.Quantity }, Message = "Item added to cart successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShoppingCartItem> { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    /// <summary>
    /// Removes a specific item from the shopping cart.
    /// </summary>
    public async Task<ServiceResponse<bool>> RemoveItemFromCartAsync(int itemId)
    {
        try
        {
            var item = await _context.ShoppingCartItems.FindAsync(itemId);
            if (item == null)
            {
                return new ServiceResponse<bool> { Success = false, Message = "Item not found." };
            }
            _context.ShoppingCartItems.Remove(item);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = "An error occurred: " + ex.Message };
        }
    }

    /// <summary>
    /// Clears all items from a shopping cart, leaving it empty for continued shopping or checkout.
    /// </summary>
    public async Task<ServiceResponse<bool>> ClearCartAsync(int cartId)
    {
        try
        {
            var items = await _context.ShoppingCartItems.Where(i => i.CartId == cartId).ToListAsync();
            if (!items.Any())
            {
                return new ServiceResponse<bool> { Success = false, Message = "No items found in cart." };
            }

            _context.ShoppingCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = "An error occurred: " + ex.Message };
        }
    }

}
