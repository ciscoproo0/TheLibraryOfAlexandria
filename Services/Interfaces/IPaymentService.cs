using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IPaymentService
{
    Task<ServiceResponse<Payment>> CreatePaymentAsync(Payment payment);
    Task<ServiceResponse<Payment>> GetPaymentByIdAsync(int paymentId);
    Task<ServiceResponse<Payment>> UpdatePaymentAsync(int paymentId, Payment updatedPayment);
    Task<ServiceResponse<bool>> DeletePaymentAsync(int paymentId);

}
