using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<Payment>> CreatePaymentAsync(Payment payment)
    {
        try
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return new ServiceResponse<Payment> { Data = payment, Message = "Payment created successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Payment> { Success = false, Message = $"An error occurred while creating the payment: {ex.Message}" };
        }
    }

    public async Task<ServiceResponse<Payment>> GetPaymentByIdAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new ServiceResponse<Payment> { Success = false, Message = "Payment not found." };
            }
            return new ServiceResponse<Payment> { Data = payment };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Payment> { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<ServiceResponse<Payment>> UpdatePaymentAsync(int paymentId, Payment updatedPayment)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new ServiceResponse<Payment> { Success = false, Message = "Payment not found." };
            }

            payment.Amount = updatedPayment.Amount;
            payment.Method = updatedPayment.Method;
            payment.Status = updatedPayment.Status;
            payment.TransactionId = updatedPayment.TransactionId;
            payment.CompletedAt = updatedPayment.CompletedAt;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return new ServiceResponse<Payment> { Data = payment };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Payment> { Success = false, Message = $"An error occurred while updating the payment: {ex.Message}" };
        }
    }

    public async Task<ServiceResponse<bool>> DeletePaymentAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new ServiceResponse<bool> { Success = false, Message = "Payment not found." };
            }
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true, Message = "Payment deleted successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }
}
