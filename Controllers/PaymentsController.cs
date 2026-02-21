using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// PaymentController manages payment processing and financial transaction recording.
/// Provides endpoints for creating payments, retrieving payment records, and managing payment status with advanced filtering.
/// Supports 11 payment methods (PayPal, CreditCard, DebitCard, Pix, Boleto, ApplePay, GooglePay, Venmo, Oxxo, Cash, Alternative)
/// and 6 payment statuses (Pending, Completed, Denied, Refunded, Reverted, Undefined).
/// Route: api/Payment
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Retrieves all payments with advanced 13-parameter filtering for financial analysis and reporting.
    /// Supports filtering by payment status, method, amount range, order/user IDs, date ranges, and transaction ID.
    /// Results can be sorted by Amount, Status, Method, or CreatedAt (default).
    /// </summary>
    /// <param name="status">Optional: Filter payments by status (Pending, Completed, Denied, Refunded, Reverted, Undefined)</param>
    /// <param name="method">Optional: Filter payments by method (PayPal, CreditCard, DebitCard, Pix, Boleto, ApplePay, GooglePay, Venmo, Oxxo, Cash, Alternative)</param>
    /// <param name="orderId">Optional: Filter payments associated with specific order ID</param>
    /// <param name="userId">Optional: Filter payments by user who made the payment</param>
    /// <param name="minAmount">Optional: Filter payments with minimum amount threshold (BRL currency)</param>
    /// <param name="maxAmount">Optional: Filter payments with maximum amount threshold (BRL currency)</param>
    /// <param name="createdFrom">Optional: Filter payments created on or after this date</param>
    /// <param name="createdTo">Optional: Filter payments created on or before this date</param>
    /// <param name="completedFrom">Optional: Filter payments completed on or after this date (only for completed payments)</param>
    /// <param name="completedTo">Optional: Filter payments completed on or before this date (only for completed payments)</param>
    /// <param name="transactionId">Optional: Filter payments by transaction ID (case-insensitive substring match with payment provider)</param>
    /// <param name="sortBy">Optional: Sort results by field (Amount, Status, Method, CreatedAt; default: CreatedAt)</param>
    /// <param name="sortDir">Optional: Sort direction (asc, desc; default: desc)</param>
    /// <returns>ServiceResponse containing list of payments matching all filter criteria and sort order</returns>
    /// <remarks>
    /// Authorization: Admin, ServiceAccount, and SuperAdmin only (sensitive financial data).
    /// Filter Behavior:
    /// - All filters use AND logic (all specified criteria must be satisfied)
    /// - Status and Method enums are safely parsed; invalid values default to null/skip filtering
    /// - Amount range: minAmount and maxAmount create inclusive range [minAmount, maxAmount]
    /// - Date ranges: fromDate filters inclusive >=, toDate filters inclusive <=
    /// - CompletedFrom/CompletedTo only filter payments with status=Completed
    /// - TransactionId performs case-insensitive substring matching
    /// Sorting:
    /// - Default sort: CreatedAt descending (newest first)
    /// - Valid sortBy: "Amount", "Status", "Method", "CreatedAt"
    /// - Valid sortDir: "asc" (ascending), "desc" (descending)
    /// Status Codes:
    /// - 200 OK: Payments retrieved successfully (may be empty list)
    /// - 400 BadRequest: Filter parameters invalid or service error
    /// </remarks>
    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<Payment>>>> Get(
        [FromQuery] string? status = null,
        [FromQuery] string? method = null,
        [FromQuery] int? orderId = null,
        [FromQuery] int? userId = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] DateTime? completedFrom = null,
        [FromQuery] DateTime? completedTo = null,
        [FromQuery] string? transactionId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null
    )
    {
        var resp = await _paymentService.GetPaymentsAsync(status, method, orderId, userId, minAmount, maxAmount, createdFrom, createdTo, completedFrom, completedTo, transactionId, sortBy, sortDir);
        if (!resp.Success) return BadRequest(resp);
        return Ok(resp);
    }

    /// <summary>
    /// Creates a new payment record for an order.
    /// Initially sets payment status to "Pending" and tracks payment method, amount, and transaction reference.
    /// Payment status is manually updated to "Completed" or "Denied" via PUT endpoint based on provider confirmation.
    /// </summary>
    /// <param name="payment">Payment object with orderId, userId, amount, method, and transactionId</param>
    /// <returns>Created payment record with generated ID and CreatedAt timestamp</returns>
    /// <remarks>
    /// Authorization: Customers, Admins, and SuperAdmins can create payments.
    /// Business Rules:
    /// - New payments automatically start with status "Pending"
    /// - Order must exist (FK validation on orderId)
    /// - User must exist (FK validation on userId)
    /// - Amount must be positive decimal value in BRL currency
    /// - TransactionId should contain payment provider reference (e.g., PayPal transaction ID)
    /// - Method must be one of 11 supported methods (PayPal, CreditCard, DebitCard, Pix, Boleto, ApplePay, GooglePay, Venmo, Oxxo, Cash, Alternative)
    /// Status Codes:
    /// - 201 Created: Payment created successfully, Location header contains GET endpoint
    /// - 400 BadRequest: Invalid data, order/user not found, or service error
    /// </remarks>
    [Authorize(Roles = "Customer,Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<Payment>>> CreatePayment(Payment payment)
    {
        var response = await _paymentService.CreatePaymentAsync(payment);
        if (!response.Success)
        {
            return BadRequest(response.Message);
        }
        return CreatedAtAction("GetPayment", new { id = response?.Data?.Id }, response?.Data);
    }

    /// <summary>
    /// Retrieves a single payment record by ID with complete transaction details.
    /// </summary>
    /// <param name="id">Payment ID to retrieve</param>
    /// <returns>Payment record with id, orderId, userId, amount, method, status, transactionId, and timestamps</returns>
    /// <remarks>
    /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
    /// Status Codes:
    /// - 200 OK: Payment found
    /// - 404 NotFound: Payment does not exist
    /// </remarks>
    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse<Payment>>> GetPayment(int id)
    {
        var response = await _paymentService.GetPaymentByIdAsync(id);
        if (!response.Success)
        {
            return NotFound(response.Message);
        }
        return Ok(response);
    }

    /// <summary>
    /// Updates an existing payment record, typically to reflect payment status changes from provider confirmation.
    /// Allows updating status (Pending → Completed/Denied/Refunded/Reverted), completedAt timestamp, and other fields.
    /// </summary>
    /// <param name="id">Payment ID to update</param>
    /// <param name="payment">Updated payment object with new status and fields</param>
    /// <returns>204 NoContent on success</returns>
    /// <remarks>
    /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
    /// Business Rules:
    /// - Status transitions: typically Pending → Completed or Pending → Denied
    /// - CompletedAt timestamp should be set when status changes to "Completed"
    /// - UpdatedAt timestamp is automatically refreshed on successful update
    /// - CreatedAt timestamp is preserved and never modified
    /// Status Codes:
    /// - 204 NoContent: Payment updated successfully
    /// - 400 BadRequest: Invalid status transition or service error
    /// - 404 NotFound: Payment does not exist
    /// </remarks>
    [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, Payment payment)
    {
        var response = await _paymentService.UpdatePaymentAsync(id, payment);
        if (!response.Success)
        {
            return BadRequest(response.Message);
        }
        return NoContent();
    }

    /// <summary>
    /// Deletes a payment record from the system.
    /// This is a hard delete operation that removes the payment history; consider marking as "Reverted" instead for audit trail.
    /// </summary>
    /// <param name="id">Payment ID to delete</param>
    /// <returns>204 NoContent on success</returns>
    /// <remarks>
    /// Authorization: SuperAdmin only (most restrictive operation).
    /// Important Considerations:
    /// - This is a destructive operation that cannot be undone
    /// - Deletes payment history and provider transaction references
    /// - For audit trails and financial reconciliation, consider marking status="Reverted" instead of deleting
    /// - If order still references this payment via FK, deletion may fail
    /// Status Codes:
    /// - 204 NoContent: Payment deleted successfully
    /// - 404 NotFound: Payment does not exist
    /// </remarks>
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var response = await _paymentService.DeletePaymentAsync(id);
        if (!response.Success)
        {
            return NotFound(response.Message);
        }
        return NoContent();
    }
}
