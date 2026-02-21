using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IProductService defines the contract for product catalog management operations.
/// This service handles CRUD operations on Magic: The Gathering products including
/// search, filtering by Magic properties (rarity, edition), and inventory management.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products with optional filtering by Magic attributes, price, and stock levels.
    /// Supports full-text search and filtering for product discovery and filtering.
    /// </summary>
    /// <param name="search">Optional: Keyword search across product Name and Description fields.</param>
    /// <param name="rarity">Optional: Filter by card rarity (e.g., "Common", "Uncommon", "Rare", "Mythic Rare").</param>
    /// <param name="edition">Optional: Filter by Magic set edition/release (e.g., "Dominaria United", "The Brothers' War").</param>
    /// <param name="minPrice">Optional: Filter products with price greater than or equal to this value.</param>
    /// <param name="maxPrice">Optional: Filter products with price less than or equal to this value.</param>
    /// <param name="minStock">Optional: Filter products with stock quantity greater than or equal to this value.</param>
    /// <param name="maxStock">Optional: Filter products with stock quantity less than or equal to this value.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of Product objects matching all filter criteria
    /// - Failure: Error message if query fails
    /// </returns>
    /// <remarks>
    /// All parameters are optional and can be combined for precise filtering.
    /// Search is case-insensitive and matches partial strings.
    /// Stock filtering is useful for finding inventory gaps or overstocked items.
    /// </remarks>
    Task<ServiceResponse<List<Product>>> GetAllProductsAsync(
        string? search = null,
        string? rarity = null,
        string? edition = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minStock = null,
        int? maxStock = null
    );

    /// <summary>
    /// Retrieves a single product by its ID with complete details.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Complete Product object with all properties
    /// - Failure: Error message if product not found
    /// </returns>
    Task<ServiceResponse<Product>> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a new product in the catalog.
    /// </summary>
    /// <param name="product">The Product object containing product details (Name, Description, Price, StockQuantity, etc.).</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created Product object with generated ID and timestamps
    /// - Failure: Error message if product creation fails (e.g., duplicate name, validation errors)
    /// </returns>
    /// <remarks>
    /// Product Name and Description are required fields subject to validation.
    /// Price must be positive. StockQuantity must be non-negative.
    /// CreatedAt and UpdatedAt timestamps are automatically set to current UTC time.
    /// </remarks>
    Task<ServiceResponse<Product>> CreateProductAsync(Product product);

    /// <summary>
    /// Updates an existing product with new information.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="product">The updated Product object with new values.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Updated Product object
    /// - Failure: Error message if product not found or update fails
    /// </returns>
    /// <remarks>
    /// UpdatedAt timestamp is automatically refreshed to current UTC time.
    /// CreatedAt remains unchanged to preserve original product creation date.
    /// Price and stock updates affect all pending and future orders using this product.
    /// </remarks>
    Task<ServiceResponse<Product>> UpdateProductAsync(int id, Product product);

    /// <summary>
    /// Deletes a product from the catalog.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if deletion succeeded
    /// - Failure: Error message if product not found or deletion fails
    /// </returns>
    /// <remarks>
    /// Deletion may be prevented if product is referenced in active orders or shopping carts.
    /// Consider marking products as inactive instead of deleting them to preserve historical data.
    /// </remarks>
    Task<ServiceResponse<bool>> DeleteProductAsync(int id);

}
