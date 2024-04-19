using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IUserFavoriteService
{
    Task<ServiceResponse<List<UserFavorite>>> GetUserFavoritesAsync(int userId);
    Task<ServiceResponse<UserFavorite>> GetFavoriteByIdAsync(int favoriteId);
    Task<ServiceResponse<UserFavorite>> AddFavoriteAsync(UserFavorite favorite);
    Task<ServiceResponse<bool>> RemoveFavoriteAsync(int favoriteId);
}
