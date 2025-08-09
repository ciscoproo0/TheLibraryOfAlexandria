using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IPaymentService
{
    Task<ServiceResponse<Payment>> CreatePaymentAsync(Payment payment);
    Task<ServiceResponse<Payment>> GetPaymentByIdAsync(int paymentId);
    Task<ServiceResponse<Payment>> GetPaymentByOrderIdAsync(int orderId);
    Task<ServiceResponse<Payment>> UpdatePaymentAsync(int paymentId, Payment updatedPayment);
    Task<ServiceResponse<bool>> DeletePaymentAsync(int paymentId);
    Task<ServiceResponse<List<Payment>>> GetAllPaymentsAsync();
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
