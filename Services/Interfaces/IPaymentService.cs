using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IPaymentService defines the contract for payment processing and management operations.
/// This service handles payment CRUD operations, filtering for financial reporting,
/// and payment status tracking across multiple payment methods and gateway providers.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a new payment record linked to an order.
    /// </summary>
    /// <param name="payment">The Payment object containing order ID, amount, method, and transaction details.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created Payment object with generated ID and timestamps
    /// - Failure: Error message if payment creation fails (e.g., invalid order, validation errors)
    /// </returns>
    /// <remarks>
    /// Initial payment status is "Pending" until payment gateway confirmation received.
    /// TransactionId from payment provider must be unique per payment for idempotency.
    /// CreatedAt timestamp is automatically set to current UTC time.
    /// </remarks>
    Task<ServiceResponse<Payment>> CreatePaymentAsync(Payment payment);

    /// <summary>
    /// Retrieves a single payment by its ID.
    /// </summary>
    /// <param name="paymentId">The unique identifier of the payment to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Complete Payment object with status history
    /// - Failure: Error message if payment not found
    /// </returns>
    Task<ServiceResponse<Payment>> GetPaymentByIdAsync(int paymentId);

    /// <summary>
    /// Retrieves the payment associated with a specific order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Payment object linked to the order
    /// - Failure: Error message if order not found or has no payment
    /// </returns>
    /// <remarks>
    /// Orders can have multiple payments if refunds are processed separately.
    /// This method typically returns the primary payment; use GetPaymentsAsync for full history.
    /// </remarks>
    Task<ServiceResponse<Payment>> GetPaymentByOrderIdAsync(int orderId);

    /// <summary>
    /// Updates an existing payment record with new information.
    /// </summary>
    /// <param name="paymentId">The unique identifier of the payment to update.</param>
    /// <param name="updatedPayment">The updated Payment object with new values.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Updated Payment object
    /// - Failure: Error message if payment not found or update fails
    /// </returns>
    /// <remarks>
    /// Status changes update the CompletedAt timestamp appropriately.
    /// Only status changes may be permissible after initial creation (not amount, method).
    /// </remarks>
    Task<ServiceResponse<Payment>> UpdatePaymentAsync(int paymentId, Payment updatedPayment);

    /// <summary>
    /// Deletes a payment record from the system.
    /// </summary>
    /// <param name="paymentId">The unique identifier of the payment to delete.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if deletion succeeded
    /// - Failure: Error message if payment not found or deletion fails
    /// </returns>
    /// <remarks>
    /// Payment deletion may be restricted based on completion status for audit trail compliance.
    /// </remarks>
    Task<ServiceResponse<bool>> DeletePaymentAsync(int paymentId);

    /// <summary>
    /// Retrieves all payments without filtering.
    /// </summary>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of all Payment objects
    /// - Failure: Error message if query fails
    /// </returns>
    Task<ServiceResponse<List<Payment>>> GetAllPaymentsAsync();

    /// <summary>
    /// Retrieves payments with advanced filtering for financial reporting and analysis.
    /// Supports filtering by 13 different criteria for comprehensive payment queries.
    /// </summary>
    /// <param name="status">Optional: Filter by payment status (e.g., "Pending", "Completed", "Denied", "Refunded").</param>
    /// <param name="method">Optional: Filter by payment method (e.g., "CreditCard", "PayPal", "Pix", "DebitCard", etc.).</param>
    /// <param name="orderId">Optional: Filter payments linked to a specific order ID.</param>
    /// <param name="userId">Optional: Filter payments from orders by specific user ID.</param>
    /// <param name="minAmount">Optional: Filter payments with transaction amount greater than or equal to this value.</param>
    /// <param name="maxAmount">Optional: Filter payments with transaction amount less than or equal to this value.</param>
    /// <param name="createdFrom">Optional: Filter payments created on or after this date (UTC).</param>
    /// <param name="createdTo">Optional: Filter payments created on or before this date (UTC).</param>
    /// <param name="completedFrom">Optional: Filter payments completed on or after this date (UTC).</param>
    /// <param name="completedTo">Optional: Filter payments completed on or before this date (UTC).</param>
    /// <param name="transactionId">Optional: Filter by exact transaction ID from payment provider.</param>
    /// <param name="sortBy">Optional: Field to sort results by (e.g., "Amount", "CreatedAt", "Status").</param>
    /// <param name="sortDir">Optional: Sort direction ("ASC" for ascending, "DESC" for descending).</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of Payment objects matching all filter criteria, sorted as requested
    /// - Failure: Error message if query fails
    /// </returns>
    /// <remarks>
    /// All parameters are optional. Multiple filters use AND logic (all must match).
    /// Date filtering uses UTC for consistency across timezones.
    /// Useful for financial reports: daily/monthly revenue, payment method breakdown, failed payment tracking.
    /// Empty result list is valid response for filters matching no payments.
    /// </remarks>
    Task<ServiceResponse<List<Payment>>> GetPaymentsAsync(
        string? status,
        string? method,
        int? orderId,
        int? userId,
        decimal? minAmount,
        decimal? maxAmount,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? completedFrom,
        DateTime? completedTo,
        string? transactionId,
        string? sortBy,
        string? sortDir
    );

}
