using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public class ShippingInfoService : IShippingInfoService
{
    private readonly ApplicationDbContext _context;

    public ShippingInfoService(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public async Task<ServiceResponse<ShippingInfo>> UpdateShippingInfoAsync(int id, ShippingInfo updated)
    {
        try
        {
            var entity = await _context.ShippingInfos.FindAsync(id);
            if (entity == null)
            {
                return new ServiceResponse<ShippingInfo> { Success = false, Message = "Shipping info not found." };
            }

            entity.Status = updated.Status;
            entity.TrackingNumber = string.IsNullOrWhiteSpace(updated.TrackingNumber) ? entity.TrackingNumber : updated.TrackingNumber;
            entity.ShippingCost = updated.ShippingCost;

            await _context.SaveChangesAsync();
            return new ServiceResponse<ShippingInfo> { Data = entity, Message = "Shipping info updated successfully." };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<ShippingInfo> { Success = false, Message = ex.Message };
        }
    }

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


