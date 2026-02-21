using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// ProductService implements product catalog management with search and filtering.
/// Supports CRUD operations on Magic: The Gathering products with multi-criteria filtering
/// by name, rarity, edition, price range, and stock quantities.
/// </summary>
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes ProductService with database context.
    /// </summary>
    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves products with optional multi-criteria filtering for product discovery.
    /// Supports search across Name and Description, rarity/edition filtering, and price/stock ranges.
    /// Results ordered alphabetically by product name.
    /// </summary>
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

            // Filter by text search (case-insensitive partial match on Name or Description)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(s) || p.Description.ToLower().Contains(s));
            }
            // Filter by Magic card rarity
            if (!string.IsNullOrWhiteSpace(rarity))
            {
                query = query.Where(p => p.Rarity == rarity);
            }
            // Filter by Magic set edition
            if (!string.IsNullOrWhiteSpace(edition))
            {
                query = query.Where(p => p.Edition == edition);
            }
            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            // Filter by stock quantity range (useful for inventory management)
            if (minStock.HasValue)
            {
                query = query.Where(p => p.StockQuantity >= minStock.Value);
            }
            if (maxStock.HasValue)
            {
                query = query.Where(p => p.StockQuantity <= maxStock.Value);
            }

            // Sort alphabetically for consistent browsing experience
            query = query.OrderBy(p => p.Name);
            var products = await query.ToListAsync();
            return new ServiceResponse<List<Product>> { Data = products };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<Product>> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Retrieves a single product by ID with complete details.
    /// </summary>
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

    /// <summary>
    /// Creates a new product in the catalog with validation.
    /// </summary>
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

    /// <summary>
    /// Updates an existing product with new information and refreshes UpdatedAt timestamp.
    /// </summary>
    public async Task<ServiceResponse<Product>> UpdateProductAsync(int id, Product product)
    {
        try
        {
            var findProduct = await _context.Products.FindAsync(id);
            if (findProduct == null)
            {
                return new ServiceResponse<Product> { Success = false, Message = "Product not found" };
            }

            // Update all product fields
            findProduct.Name = product.Name;
            findProduct.Description = product.Description;
            findProduct.ImageUrl = product.ImageUrl;
            findProduct.Price = product.Price;
            findProduct.StockQuantity = product.StockQuantity;
            findProduct.Edition = product.Edition;
            findProduct.Rarity = product.Rarity;
            // Refresh UpdatedAt to current UTC time
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

    /// <summary>
    /// Deletes a product from the catalog.
    /// </summary>
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
