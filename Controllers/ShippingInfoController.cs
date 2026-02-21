using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using TheLibraryOfAlexandria.Services;

namespace TheLibraryOfAlexandria.Controllers
{
    /// <summary>
    /// ShippingInfoController manages shipping information and logistics tracking for orders.
    /// Provides endpoints for creating, updating, and retrieving shipping details with status lifecycle management.
    /// Shipping status progresses through: Preparing → Shipped → Delivered.
    /// Includes order-specific retrieval and tracking number management.
    /// Route: api/ShippingInfo
    /// </summary>
    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingInfoController : ControllerBase
    {
        private readonly IShippingInfoService _service;

        public ShippingInfoController(IShippingInfoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all shipping records in the system.
        /// Returns complete shipping information for all orders with status and tracking details.
        /// </summary>
        /// <returns>List of all ShippingInfo records</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only (sensitive shipping data).
        /// Status Codes:
        /// - 200 OK: Shipping records retrieved successfully (may be empty list)
        /// - 400 BadRequest: Service error during retrieval
        /// </remarks>
        // GET: api/ShippingInfo
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<ShippingInfo>>>> GetAll()
        {
            var resp = await _service.GetAllShippingInfosAsync();
            if (!resp.Success) return BadRequest(resp);
            return Ok(resp);
        }

        /// <summary>
        /// Retrieves a single shipping record by ID with complete tracking and status details.
        /// </summary>
        /// <param name="id">ShippingInfo ID to retrieve</param>
        /// <returns>ShippingInfo record with order ID, status, tracking number, and timestamps</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
        /// Status Codes:
        /// - 200 OK: Shipping record found
        /// - 404 NotFound: ShippingInfo does not exist
        /// </remarks>
        // GET: api/ShippingInfo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> GetById(int id)
        {
            var resp = await _service.GetShippingInfoByIdAsync(id);
            if (!resp.Success) return NotFound(resp);
            return Ok(resp);
        }

        /// <summary>
        /// Retrieves shipping information for a specific order by Order ID.
        /// Enables order-centric queries for checking shipping status without knowing ShippingInfo ID.
        /// </summary>
        /// <param name="orderId">Order ID for which to retrieve shipping information</param>
        /// <returns>ShippingInfo for the specified order</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
        /// Status Codes:
        /// - 200 OK: Shipping record found
        /// - 404 NotFound: No shipping information exists for this order
        /// </remarks>
        // GET: api/ShippingInfo/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> GetByOrderId(int orderId)
        {
            var resp = await _service.GetShippingInfoByOrderIdAsync(orderId);
            if (!resp.Success) return NotFound(resp);
            return Ok(resp);
        }

        /// <summary>
        /// Creates a new shipping record for an order.
        /// Initial status is "Preparing"; transitions to "Shipped" when item begins shipping, then "Delivered" on receipt.
        /// Associates shipping costs and carrier tracking information with an order.
        /// </summary>
        /// <param name="model">ShippingInfo with OrderId and shipping details</param>
        /// <returns>Created ShippingInfo record with generated ID and CreatedAt timestamp</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
        /// Business Rules:
        /// - New shipping records start with status "Preparing"
        /// - Order must exist (FK validation on OrderId)
        /// - One shipping record per order (if FK constraint enforces uniqueness)
        /// - Tracking number and shipping cost are populated at creation
        /// Status Codes:
        /// - 201 Created: ShippingInfo created successfully, Location header contains GET endpoint
        /// - 400 BadRequest: Invalid data, order not found, or duplicate shipping info for order
        /// </remarks>
        // POST: api/ShippingInfo
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> Create([FromBody] ShippingInfo model)
        {
            var resp = await _service.CreateShippingInfoAsync(model);
            if (!resp.Success) return BadRequest(resp);
            return CreatedAtAction(nameof(GetById), new { id = resp.Data.Id }, resp);
        }

        /// <summary>
        /// Updates shipping information with selective field updates for status progression and tracking updates.
        /// Only Status, TrackingNumber, and ShippingCost fields are updated from request (other fields ignored).
        /// Tracking number is preserved if not explicitly provided in update (null = keep existing).
        /// </summary>
        /// <param name="id">ShippingInfo ID to update</param>
        /// <param name="request">UpdateShippingRequest with Status, TrackingNumber, and/or ShippingCost to update</param>
        /// <returns>Updated ShippingInfo record with refreshed timestamp</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
        /// Business Rules:
        /// - Status can transition: Preparing → Shipped → Delivered
        /// - Status defaults to "Preparing" if null in request
        /// - TrackingNumber: if provided, updates existing value; if null, preserves existing value
        /// - ShippingCost: can be updated; defaults to 0 if not provided
        /// - UpdatedAt is automatically refreshed on successful update
        /// - CreatedAt is preserved and never modified
        /// Status Codes:
        /// - 200 OK: Shipping info updated successfully
        /// - 400 BadRequest: Invalid status transition or service error
        /// - 404 NotFound: ShippingInfo does not exist
        /// </remarks>
        // PUT: api/ShippingInfo/{id}
        // Updates only selected fields (Status, TrackingNumber, ShippingCost)
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<ShippingInfo>>> Put(int id, [FromBody] UpdateShippingRequest request)
        {
            var placeholder = new ShippingInfo
            {
                Status = request.Status ?? ShippingStatus.Preparing,
                TrackingNumber = request.TrackingNumber ?? string.Empty,
                ShippingCost = request.ShippingCost ?? 0
            };
            var resp = await _service.UpdateShippingInfoAsync(id, placeholder);
            if (!resp.Success) return BadRequest(resp);
            return Ok(resp);
        }

        /// <summary>
        /// Deletes a shipping record from the system.
        /// Hard delete that removes shipping history and tracking information.
        /// Consider marking status as "Cancelled" instead for audit trail preservation.
        /// </summary>
        /// <param name="id">ShippingInfo ID to delete</param>
        /// <returns>204 NoContent on success</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
        /// Important Considerations:
        /// - This is a destructive operation that cannot be undone
        /// - Deletes shipping history and carrier tracking information
        /// - For audit trails and logistics reconciliation, consider soft-deleting instead
        /// - May fail if Order still has FK reference to this shipping info
        /// Status Codes:
        /// - 204 NoContent: ShippingInfo deleted successfully
        /// - 404 NotFound: ShippingInfo does not exist
        /// </remarks>
        // DELETE: api/ShippingInfo/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resp = await _service.DeleteShippingInfoAsync(id);
            if (!resp.Success) return NotFound(resp);
            return NoContent();
        }
    }

    /// <summary>
    /// UpdateShippingRequest is a data transfer object (DTO) for partial shipping updates.
    /// Allows selective updates to Status, TrackingNumber, and ShippingCost without modifying other fields.
    /// All fields are optional; null values preserve existing data (except Status which defaults to Preparing).
    /// </summary>
    public class UpdateShippingRequest
    {
        /// <summary>
        /// New shipping status (Preparing, Shipped, Delivered). Optional; defaults to Preparing if not provided.
        /// </summary>
        public ShippingStatus? Status { get; set; }

        /// <summary>
        /// Carrier tracking number for shipment. Optional; if null, existing value is preserved.
        /// </summary>
        public string? TrackingNumber { get; set; }

        /// <summary>
        /// Shipping cost in BRL currency. Optional; defaults to 0 if not provided.
        /// </summary>
        public decimal? ShippingCost { get; set; }
    }
}



