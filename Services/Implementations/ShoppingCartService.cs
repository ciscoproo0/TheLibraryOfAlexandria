using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public class ShoppingCartService : IShoppingCartService
{
    private readonly ApplicationDbContext _context;

    public ShoppingCartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<ShoppingCart>> CreateCartForUserAsync(int userId)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Id == userId) == false)
            {
                return new ServiceResponse<ShoppingCart> { Success = false, Message = "User not found." };
            }

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

    public async Task<ServiceResponse<ShoppingCartItem>> AddItemToCartAsync(int cartId, ShoppingCartItem itemDto)
    {
        try
        {
            var cart = await _context.ShoppingCarts.FindAsync(cartId);
            if (cart == null)
            {
                return new ServiceResponse<ShoppingCartItem> { Success = false, Message = "Cart not found." };
            }

            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                return new ServiceResponse<ShoppingCartItem> { Success = false, Message = "Product not found." };
            }

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
