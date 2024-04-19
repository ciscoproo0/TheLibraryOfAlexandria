using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<List<Product>>> GetAllProductsAsync()
    {
        try
        {
            var products = await _context.Products.ToListAsync();
            return new ServiceResponse<List<Product>> { Data = products };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<Product>> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<Product>> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return new ServiceResponse<Product> { Success = false, Message = "Product not found." };

            return new ServiceResponse<Product> { Data = product };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Product> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<Product>> CreateProductAsync(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return new ServiceResponse<Product> { Data = product };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Product> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<Product>> UpdateProductAsync(int id, Product product)
    {
        try
        {
            var findProduct = await _context.Products.FindAsync(id);
            if (findProduct == null)
            {
                return new ServiceResponse<Product> { Success = false, Message = "Product not found" };
            }

            findProduct.Name = product.Name;
            findProduct.Description = product.Description;
            findProduct.ImageUrl = product.ImageUrl;
            findProduct.Price = product.Price;
            findProduct.StockQuantity = product.StockQuantity;
            findProduct.Edition = product.Edition;
            findProduct.Rarity = product.Rarity;
            findProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new ServiceResponse<Product> { Data = findProduct };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new ServiceResponse<Product> { Success = false, Message = "Failed to update the product. " + ex.Message };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Product> { Success = false, Message = "An error occurred while updating the product. " + ex.Message };
        }
    }

    public async Task<ServiceResponse<bool>> DeleteProductAsync(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return new ServiceResponse<bool> { Success = false, Message = "Product not found." };

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = ex.Message };
        }
    }
}
