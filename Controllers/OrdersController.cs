using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using TheLibraryOfAlexandria.Services;


namespace TheLibraryOfAlexandria.Controllers
{
    /// <summary>
    /// OrdersController manages order lifecycle operations and retrieval with advanced filtering.
    /// Provides endpoints for browsing, creating, and managing orders with multi-criteria filtering
    /// by user ID, status, total price, and creation/update date ranges.
    /// Customers can create orders and view their own; Admins manage all orders.
    /// Route: api/Orders
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Retrieves all orders with optional multi-criteria filtering for order discovery and management.
        /// Supports filtering by user ID, status, total price range, and creation/update date ranges.
        /// Results include related OrderItems, ShippingInfo, and Payment aggregated from related services.
        /// </summary>
        /// <param name="userId">Optional: Filter orders by specific user ID</param>
        /// <param name="status">Optional: Filter orders by status (e.g., "Pending", "Completed", "Cancelled")</param>
        /// <param name="minTotalPrice">Optional: Filter orders with minimum total price threshold</param>
        /// <param name="maxTotalPrice">Optional: Filter orders with maximum total price threshold</param>
        /// <param name="createdFrom">Optional: Filter orders created on or after this date</param>
        /// <param name="createdTo">Optional: Filter orders created on or before this date</param>
        /// <param name="updatedFrom">Optional: Filter orders updated on or after this date</param>
        /// <param name="updatedTo">Optional: Filter orders updated on or before this date</param>
        /// <returns>ServiceResponse containing list of orders matching all filter criteria</returns>
        /// <remarks>
        /// Authorization: Customers, Admins, and SuperAdmins can access this endpoint.
        /// Status Codes:
        /// - 200 OK: Orders retrieved successfully (may be empty list)
        /// - 400 BadRequest: Filter parameters invalid (e.g., minPrice > maxPrice, invalid date format)
        /// </remarks>
        // GET: api/Orders (filters preserved)
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<Order>>>> GetOrders(
            [FromQuery] int? userId,
            [FromQuery] string? status,
            [FromQuery] decimal? minTotalPrice,
            [FromQuery] decimal? maxTotalPrice,
            [FromQuery] DateTime? createdFrom,
            [FromQuery] DateTime? createdTo,
            [FromQuery] DateTime? updatedFrom,
            [FromQuery] DateTime? updatedTo
        )
        {
            var response = await _orderService.GetAllOrdersAsync(userId, status, minTotalPrice, maxTotalPrice, createdFrom, createdTo, updatedFrom, updatedTo);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a single order by ID with complete details including related items, shipping, and payment information.
        /// Aggregates order data with payment and shipping records from related services to provide a unified order view.
        /// This endpoint constructs a comprehensive response object containing all order lifecycle information.
        /// </summary>
        /// <param name="id">Order ID to retrieve</param>
        /// <returns>
        /// Order object with aggregated fields:
        /// - id, userId, status, totalPrice, createdAt, updatedAt
        /// - orderItems: Array of items in the order
        /// - shippingInfo: Shipping details (null if not yet created)
        /// - payment: Payment details (null if not yet processed)
        /// </returns>
        /// <remarks>
        /// Authorization: Customers, Admins, and SuperAdmins can access this endpoint.
        /// Important: The payment and shipping information are fetched from separate services and aggregated.
        /// If payment/shipping services are unavailable, those fields will be null but the request won't fail.
        /// Status Codes:
        /// - 200 OK: Order found with complete details
        /// - 404 NotFound: Order not found
        /// - 400 BadRequest: Service error occurred during aggregation
        /// </remarks>
        // GET: api/Orders/5
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            var response = await _orderService.GetOrderByIdAsync(id);
            if (response.Success)
            {
                if (response.Data == null)
                {
                    return NotFound("Order not found.");
                }
                // Try to get payment; if not found, return null node
                Payment? payment = null;
                try
                {
                    var payResp = await HttpContext.RequestServices.GetRequiredService<IPaymentService>().GetPaymentByOrderIdAsync(id);
                    if (payResp.Success)
                    {
                        payment = payResp.Data;
                    }
                }
                catch {}

                var order = response.Data;
                // If shipping not present, ensure it goes as null (empty node)
                var shipping = order.ShippingInfo; // may be null

                return Ok(new
                {
                    id = order.Id,
                    userId = order.UserId,
                    status = order.Status,
                    totalPrice = order.TotalPrice,
                    createdAt = order.CreatedAt,
                    updatedAt = order.UpdatedAt,
                    orderItems = order.OrderItems,
                    shippingInfo = shipping,
                    payment = payment
                });
            }
            else
            {
                return BadRequest(response.Message);
            }
        }

        /// <summary>
        /// Creates a new order with order items and calculates total price based on item quantities and prices.
        /// The order starts in "Pending" status and must progress through payment and shipping before completion.
        /// Stock inventory is validated for all items before order creation.
        /// </summary>
        /// <param name="order">Order object with UserId, OrderItems array, and other order details</param>
        /// <returns>Created order with generated ID and timestamp</returns>
        /// <remarks>
        /// Authorization: Customers, Admins, and SuperAdmins can create orders.
        /// Business Rules:
        /// - All items must have sufficient stock quantity available
        /// - TotalPrice is calculated from OrderItems (Price * Quantity per item)
        /// - New orders automatically start with status "Pending"
        /// - Stock inventory is decremented upon successful order creation
        /// - If any item is out of stock, the entire order creation fails with rollback
        /// Status Codes:
        /// - 201 Created: Order created successfully, Location header contains GET endpoint
        /// - 400 BadRequest: Invalid order data, stock unavailable, or calculation error
        /// </remarks>
        // POST: api/Orders
        [Authorize(Roles = "Customer, Admin, SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            var response = await _orderService.CreateOrderAsync(order);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return CreatedAtAction(nameof(GetOrder), new { id = response.Data.Id }, response.Data);

        }

        /// <summary>
        /// Updates an existing order's status with business rule validation for order lifecycle progression.
        /// Orders can only reach "Completed" status if both payment is "Completed" and shipping is "Delivered".
        /// This ensures orders follow the proper lifecycle: Pending → Payment Processing → Shipped → Completed.
        /// </summary>
        /// <param name="id">Order ID to update</param>
        /// <param name="order">Updated order object with new status (only Status field is updated from the request)</param>
        /// <returns>204 NoContent on success, indicating the order was updated</returns>
        /// <remarks>
        /// Authorization: Admins and SuperAdmins only (sensitive operation affecting order state).
        /// Business Rules:
        /// - Status transitions must be valid (e.g., cannot go backward from Completed to Pending)
        /// - To reach "Completed" status: require payment.status == "Completed" AND shipping.status == "Delivered"
        /// - UpdatedAt timestamp is automatically refreshed on successful update
        /// - CreatedAt timestamp is preserved and never modified
        /// - Request body only binds Status field; other Order fields are ignored
        /// Validation:
        /// - Request path ID must match Order.Id in body
        /// Status Codes:
        /// - 204 NoContent: Order updated successfully
        /// - 400 BadRequest: Invalid status transition, business rule violation, or ID mismatch
        /// - 404 NotFound: Order does not exist
        /// </remarks>
        // PUT: api/Orders/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutOrder(int id, [FromBody][Bind("Status")] Order order)
        {
            // Validate that the route ID matches the order ID
            if (id != order.Id)
            {
                return BadRequest("Mismatched order ID");
            }

            var response = await _orderService.UpdateOrderAsync(id, order);
            if (!response.Success)
            {
                // Return structured JSON instead of plain text
                if (string.Equals(response.Message, "Order not found.", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }
            return NoContent(); // 204 No Content indicates update succeeded

        }

        /// <summary>
        /// Deletes an order from the system.
        /// Deletion is a hard delete that removes the order and all related OrderItems from the database.
        /// Shipping and Payment records associated with the order are cascade-deleted if configured.
        /// Consider marking orders as "Cancelled" instead of deletion for order history preservation.
        /// </summary>
        /// <param name="id">Order ID to delete</param>
        /// <returns>204 NoContent on success, indicating the order was deleted</returns>
        /// <remarks>
        /// Authorization: Admins and SuperAdmins only (sensitive operation).
        /// Important Considerations:
        /// - This is a destructive operation that cannot be undone
        /// - For audit trails and business intelligence, consider soft-deleting (mark as Cancelled) instead
        /// - All OrderItems associated with this order are also deleted
        /// - Related Payment and ShippingInfo records may be cascade-deleted depending on DbContext configuration
        /// Status Codes:
        /// - 204 NoContent: Order deleted successfully
        /// - 404 NotFound: Order does not exist
        /// </remarks>
        // DELETE: api/Orders/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var response = await _orderService.DeleteOrderAsync(id);
            if (!response.Success)
            {
                return NotFound(response.Message);
            }
            return NoContent(); // Returns 204 No Content to indicate successful deletion

        }
    }
}
