using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IOrderService defines the contract for order management operations.
/// This service handles order CRUD operations, filtering, and business logic for order processing.
/// Orders represent customer purchases and include linked payment and shipping information.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Retrieves all orders with optional filtering by multiple criteria.
    /// Supports filtering by user, status, price range, and date ranges for advanced reporting.
    /// </summary>
    /// <param name="userId">Optional: Filter orders by specific user ID. Null returns orders for all users.</param>
    /// <param name="status">Optional: Filter orders by status (e.g., "Pending", "Completed", "Cancelled").</param>
    /// <param name="minTotalPrice">Optional: Filter orders with total price greater than or equal to this value.</param>
    /// <param name="maxTotalPrice">Optional: Filter orders with total price less than or equal to this value.</param>
    /// <param name="createdFrom">Optional: Filter orders created on or after this date (UTC).</param>
    /// <param name="createdTo">Optional: Filter orders created on or before this date (UTC).</param>
    /// <param name="updatedFrom">Optional: Filter orders last modified on or after this date (UTC).</param>
    /// <param name="updatedTo">Optional: Filter orders last modified on or before this date (UTC).</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of Order objects matching the filter criteria
    /// - Failure: Error message if query fails
    /// </returns>
    /// <remarks>
    /// All date/time parameters use UTC for consistency.
    /// Multiple filters can be combined (AND logic) for precise queries.
    /// </remarks>
    Task<ServiceResponse<List<Order>>> GetAllOrdersAsync(
        int? userId,
        string? status,
        decimal? minTotalPrice,
        decimal? maxTotalPrice,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? updatedFrom,
        DateTime? updatedTo
    );

    /// <summary>
    /// Retrieves a single order by its ID along with associated payment and shipping information.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Complete Order object with related Payment and ShippingInfo included
    /// - Failure: Error message if order not found
    /// </returns>
    Task<ServiceResponse<Order>> GetOrderByIdAsync(int orderId);

    /// <summary>
    /// Creates a new order with associated order items, payment, and shipping information.
    /// </summary>
    /// <param name="order">The Order object containing order details, items, payment, and shipping info.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created Order object with generated ID and timestamps
    /// - Failure: Error message if order creation fails (e.g., invalid user, product out of stock)
    /// </returns>
    /// <remarks>
    /// Order creation is transactional: if any part fails (payment, shipping, items), the entire order is rolled back.
    /// Stock quantities are decremented for each order item.
    /// Timestamps (CreatedAt, UpdatedAt) are automatically set to current UTC time.
    /// </remarks>
    Task<ServiceResponse<Order>> CreateOrderAsync(Order order);

    /// <summary>
    /// Updates an existing order with new information.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to update.</param>
    /// <param name="order">The updated Order object with new values.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Updated Order object
    /// - Failure: Error message if order not found or update fails
    /// </returns>
    /// <remarks>
    /// UpdatedAt timestamp is automatically refreshed to current UTC time.
    /// Some order properties may be immutable (e.g., CreatedAt, order items) depending on order status.
    /// </remarks>
    Task<ServiceResponse<Order>> UpdateOrderAsync(int orderId, Order order);

    /// <summary>
    /// Deletes an order from the database.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to delete.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if deletion succeeded
    /// - Failure: Error message if order not found or deletion fails
    /// </returns>
    /// <remarks>
    /// Deletion cascades to associated OrderItems, Payment, and ShippingInfo records.
    /// Orders with "Shipped" or "Delivered" status may not be deletable depending on business rules.
    /// </remarks>
    Task<ServiceResponse<bool>> DeleteOrderAsync(int orderId);
}
