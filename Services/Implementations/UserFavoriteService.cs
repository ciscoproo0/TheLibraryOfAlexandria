using Microsoft.EntityFrameworkCore;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
public class UserFavoriteService : IUserFavoriteService
{
    private readonly ApplicationDbContext _context;

    public UserFavoriteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<List<UserFavorite>>> GetUserFavoritesAsync(int userId)
    {
        try
        {
            var favorites = await _context.UserFavorites
                .Include(uf => uf.User)
                .Include(uf => uf.Product)
                .AsNoTracking()
                .Where(uf => uf.UserId == userId)
                .ToListAsync();

            return new ServiceResponse<List<UserFavorite>> { Data = favorites };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<UserFavorite>>
            {
                Success = false,
                Message = $"An error occurred while retrieving favorites: {ex.Message}"
            };
        }
    }

    public async Task<ServiceResponse<UserFavorite>> GetFavoriteByIdAsync(int favoriteId)
    {
        try
        {
            var favorite = await _context.UserFavorites
                .Include(uf => uf.User)
                .Include(uf => uf.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(uf => uf.Id == favoriteId);

            if (favorite == null)
                return new ServiceResponse<UserFavorite> { Success = false, Message = "Favorite not found." };

            return new ServiceResponse<UserFavorite> { Data = favorite };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<UserFavorite>
            {
                Success = false,
                Message = $"An error occurred while retrieving the favorite: {ex.Message}"
            };
        }
    }

    public async Task<ServiceResponse<UserFavorite>> AddFavoriteAsync(UserFavorite favorite)
    {
        var response = new ServiceResponse<UserFavorite>();
        try
        {
            _context.UserFavorites.Add(favorite);
            await _context.SaveChangesAsync();
            response.Data = favorite;
            response.Message = "Favorite added successfully.";
        }
        catch (DbUpdateException ex)
        {
            response.Success = false;
            response.Message = "An error occurred while adding the favorite: " + ex.InnerException?.Message;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "An unexpected error occurred: " + ex.Message;
        }
        return response;
    }


    public async Task<ServiceResponse<bool>> RemoveFavoriteAsync(int favoriteId)
    {
        try
        {
            var favorite = await _context.UserFavorites.FindAsync(favoriteId);
            if (favorite == null)
                return new ServiceResponse<bool> { Success = false, Message = "Favorite not found." };

            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = $"An error occurred while removing the favorite: {ex.Message}"
            };
        }
    }
}
