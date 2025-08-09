using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ServiceResponse<List<Payment>>> GetAllPaymentsAsync()
    {
        try
        {
            var list = await _context.Payments
                .Include(p => p.Order)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return new ServiceResponse<List<Payment>> { Data = list };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<Payment>> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<PaginatedResult<Payment>>> GetPaymentsAsync(
        int page,
        int pageSize,
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
    )
    {
        try
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var st))
                query = query.Where(p => p.Status == st);

            if (!string.IsNullOrWhiteSpace(method) && Enum.TryParse<PaymentMethod>(method, true, out var md))
                query = query.Where(p => p.Method == md);

            if (orderId.HasValue)
                query = query.Where(p => p.OrderId == orderId.Value);

            if (userId.HasValue)
                query = query.Where(p => p.Order != null && p.Order.UserId == userId.Value);

            if (minAmount.HasValue)
                query = query.Where(p => p.Amount >= minAmount.Value);
            if (maxAmount.HasValue)
                query = query.Where(p => p.Amount <= maxAmount.Value);

            if (createdFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(p => p.CreatedAt <= createdTo.Value);

            if (completedFrom.HasValue)
                query = query.Where(p => p.CompletedAt.HasValue && p.CompletedAt.Value >= completedFrom.Value);
            if (completedTo.HasValue)
                query = query.Where(p => p.CompletedAt.HasValue && p.CompletedAt.Value <= completedTo.Value);

            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                var like = transactionId.ToLower();
                query = query.Where(p => p.TransactionId.ToLower().Contains(like));
            }

            bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            switch (sortBy)
            {
                case "amount":
                    query = desc ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount);
                    break;
                case "status":
                    query = desc ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status);
                    break;
                case "method":
                    query = desc ? query.OrderByDescending(p => p.Method) : query.OrderBy(p => p.Method);
                    break;
                case "createdAt":
                default:
                    query = desc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
                    break;
            }

            int total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new ServiceResponse<PaginatedResult<Payment>>
            {
                Data = new PaginatedResult<Payment>
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                }
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<PaginatedResult<Payment>> { Success = false, Message = ex.Message };
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

    public async Task<ServiceResponse<Payment>> GetPaymentByOrderIdAsync(int orderId)
    {
        try
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (payment == null)
            {
                return new ServiceResponse<Payment> { Success = false, Message = "Payment not found." };
            }
            return new ServiceResponse<Payment> { Data = payment };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Payment> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<Payment>> UpdatePaymentAsync(int paymentId, Payment updatedPayment)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new ServiceResponse<Payment> { Success = false, Message = "Payment not found" };
            }

            payment.Amount = updatedPayment.Amount;
            payment.Method = updatedPayment.Method;
            payment.Status = updatedPayment.Status;
            payment.TransactionId = updatedPayment.TransactionId;
            payment.CompletedAt = updatedPayment.CompletedAt;

            await _context.SaveChangesAsync();
            return new ServiceResponse<Payment> { Data = payment };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Payment> { Success = false, Message = $"An error occurred while updating the payment. {ex.Message}" };
        }
    }

    public async Task<ServiceResponse<bool>> DeletePaymentAsync(int paymentId)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return new ServiceResponse<bool> { Success = false, Message = "Payment not found." };

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = ex.Message };
        }
    }
}
