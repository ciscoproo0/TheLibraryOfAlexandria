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

    public async Task<ServiceResponse<List<Product>>> GetAllProductsAsync(
        string? search = null,
        string? rarity = null,
        string? edition = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minStock = null,
        int? maxStock = null
    )
    {
        try
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(s) || p.Description.ToLower().Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(rarity))
            {
                query = query.Where(p => p.Rarity == rarity);
            }
            if (!string.IsNullOrWhiteSpace(edition))
            {
                query = query.Where(p => p.Edition == edition);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            if (minStock.HasValue)
            {
                query = query.Where(p => p.StockQuantity >= minStock.Value);
            }
            if (maxStock.HasValue)
            {
                query = query.Where(p => p.StockQuantity <= maxStock.Value);
            }

            query = query.OrderBy(p => p.Name);
            var products = await query.ToListAsync();
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
