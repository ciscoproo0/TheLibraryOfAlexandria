using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// ShippingInfoService implements shipping information management operations.
/// Tracks shipping details, status transitions, and delivery information for orders.
/// Supports status lifecycle: Preparing → Shipped → Delivered.
/// </summary>
public class ShippingInfoService : IShippingInfoService
{
    private readonly ApplicationDbContext _context;

    public ShippingInfoService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Creates shipping information record for an order.</summary>
    public async Task<ServiceResponse<ShippingInfo>> CreateShippingInfoAsync(ShippingInfo shippingInfo)
    {
        try
        {
            await _context.ShippingInfos.AddAsync(shippingInfo);
            await _context.SaveChangesAsync();
            return new ServiceResponse<ShippingInfo> { Data = shippingInfo, Message = "Shipping info created successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShippingInfo> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>Retrieves all shipping records for all orders.</summary>
    public async Task<ServiceResponse<List<ShippingInfo>>> GetAllShippingInfosAsync()
    {
        try
        {
            var list = await _context.ShippingInfos.ToListAsync();
            return new ServiceResponse<List<ShippingInfo>> { Data = list };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<ShippingInfo>> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>Retrieves a single shipping record by ID.</summary>
    public async Task<ServiceResponse<ShippingInfo>> GetShippingInfoByIdAsync(int id)
    {
        try
        {
            var entity = await _context.ShippingInfos.FindAsync(id);
            if (entity == null)
            {
                return new ServiceResponse<ShippingInfo> { Success = false, Message = "Shipping info not found." };
            }
            return new ServiceResponse<ShippingInfo> { Data = entity };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShippingInfo> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Updates shipping information including status transitions and tracking details.
    /// Maintains existing tracking number if not explicitly provided.
    /// </summary>
    public async Task<ServiceResponse<ShippingInfo>> UpdateShippingInfoAsync(int id, ShippingInfo updated)
    {
        try
        {
            var entity = await _context.ShippingInfos.FindAsync(id);
            if (entity == null)
            {
                return new ServiceResponse<ShippingInfo> { Success = false, Message = "Shipping info not found." };
            }

            // Update status (Preparing → Shipped → Delivered)
            entity.Status = updated.Status;
            // Update tracking number if provided, otherwise retain existing value
            entity.TrackingNumber = string.IsNullOrWhiteSpace(updated.TrackingNumber) ? entity.TrackingNumber : updated.TrackingNumber;
            // Update shipping cost
            entity.ShippingCost = updated.ShippingCost;

            await _context.SaveChangesAsync();
            return new ServiceResponse<ShippingInfo> { Data = entity, Message = "Shipping info updated successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShippingInfo> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>Deletes a shipping record from the system.</summary>
    public async Task<ServiceResponse<bool>> DeleteShippingInfoAsync(int id)
    {
        try
        {
            var entity = await _context.ShippingInfos.FindAsync(id);
            if (entity == null)
            {
                return new ServiceResponse<bool> { Success = false, Message = "Shipping info not found." };
            }
            _context.ShippingInfos.Remove(entity);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>Retrieves the shipping information for a specific order.</summary>
    public async Task<ServiceResponse<ShippingInfo>> GetShippingInfoByOrderIdAsync(int orderId)
    {
        try
        {
            var entity = await _context.ShippingInfos.FirstOrDefaultAsync(s => s.OrderId == orderId);
            if (entity == null)
            {
                return new ServiceResponse<ShippingInfo> { Success = false, Message = "Shipping info not found." };
            }
            return new ServiceResponse<ShippingInfo> { Data = entity };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShippingInfo> { Success = false, Message = ex.Message };
        }
    }
}
