using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IProductService
{
    Task<ServiceResponse<List<Product>>> GetAllProductsAsync(
        string? search = null,
        string? rarity = null,
        string? edition = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minStock = null,
        int? maxStock = null
    );
    Task<ServiceResponse<Product>> GetProductByIdAsync(int id);
    Task<ServiceResponse<Product>> CreateProductAsync(Product product);
    Task<ServiceResponse<Product>> UpdateProductAsync(int id, Product product);
    Task<ServiceResponse<bool>> DeleteProductAsync(int id);

}
