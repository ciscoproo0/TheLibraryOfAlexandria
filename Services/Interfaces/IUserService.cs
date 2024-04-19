using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

public interface IUserService
{
    Task<ServiceResponse<List<User>>> GetAllUsersAsync();
    Task<ServiceResponse<User>> GetUserByIdAsync(int id);
    Task<ServiceResponse<User>> CreateUserAsync(User user);
    Task<ServiceResponse<User>> UpdateUserAsync(int id, User user);
    Task<ServiceResponse<bool>> DeleteUserAsync(int id);
}
