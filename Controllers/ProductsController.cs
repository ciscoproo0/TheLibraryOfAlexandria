using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;

namespace TheLibraryOfAlexandria.Controllers
{
    /// <summary>
    /// ProductsController manages Magic: The Gathering product catalog operations.
    /// Provides endpoints for browsing, searching, and managing product inventory.
    /// All endpoints require Admin, ServiceAccount, or SuperAdmin authorization.
    /// Route: api/Products
    /// </summary>
    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retrieves all products with optional multi-criteria filtering.
        /// Supports search across name/description and filtering by Magic properties (rarity, edition),
        /// price range, and stock quantities.
        /// </summary>
        /// <param name="search">Optional: Search keyword for product name and description</param>
        /// <param name="rarity">Optional: Filter by card rarity</param>
        /// <param name="edition">Optional: Filter by Magic set edition</param>
        /// <param name="minPrice">Optional: Minimum price filter</param>
        /// <param name="maxPrice">Optional: Maximum price filter</param>
        /// <param name="minStock">Optional: Minimum stock quantity filter</param>
        /// <param name="maxStock">Optional: Maximum stock quantity filter</param>
        /// <returns>List of products matching all filter criteria</returns>
        /// <remarks>
        /// Status Codes:
        /// - 200 OK: Products retrieved successfully
        /// - 404 NotFound: No products found matching criteria
        /// </remarks>
        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            [FromQuery] string? search,
            [FromQuery] string? rarity,
            [FromQuery] string? edition,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? minStock,
            [FromQuery] int? maxStock
        )
        {
            var result = await _productService.GetAllProductsAsync(search, rarity, edition, minPrice, maxPrice, minStock, maxStock);
            if (result.Success)
                return Ok(result.Data);
            return NotFound(result.Message);
        }

        /// <summary>
        /// Retrieves a single product by ID with all details.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Complete product object</returns>
        /// <remarks>
        /// Status Codes:
        /// - 200 OK: Product found
        /// - 404 NotFound: Product not found
        /// </remarks>
        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new product in the catalog.
        /// </summary>
        /// <param name="product">Product object with required fields (Name, Description, Price, StockQuantity)</param>
        /// <returns>Created product with generated ID</returns>
        /// <remarks>
        /// Status Codes:
        /// - 201 Created: Product created successfully, Location header contains GET endpoint
        /// - 400 BadRequest: Invalid or missing required fields
        /// </remarks>
        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            var result = await _productService.CreateProductAsync(product);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction("GetProduct", new { id = result.Data.Id }, result.Data);
        }

        /// <summary>
        /// Updates an existing product with new information.
        /// </summary>
        /// <param name="id">Product ID to update</param>
        /// <param name="product">Updated product object</param>
        /// <returns>No content on success</returns>
        /// <remarks>
        /// Status Codes:
        /// - 204 NoContent: Product updated successfully
        /// - 400 BadRequest: Update operation failed
        /// </remarks>
        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            var result = await _productService.UpdateProductAsync(id, product);
            if (!result.Success)
                return BadRequest(result.Message);
            return NoContent();
        }

        /// <summary>
        /// Deletes a product from the catalog.
        /// Note: Consider deactivating instead of deleting to preserve order history.
        /// </summary>
        /// <param name="id">Product ID to delete</param>
        /// <returns>No content on success</returns>
        /// <remarks>
        /// Status Codes:
        /// - 204 NoContent: Product deleted successfully
        /// - 404 NotFound: Product not found
        /// </remarks>
        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return NoContent();
        }
    }
}
