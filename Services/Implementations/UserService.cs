using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordUtils _passwordUtils;

    public UserService(ApplicationDbContext context, PasswordUtils passwordUtils)
    {
        _context = context;
        _passwordUtils = passwordUtils;
    }

    public async Task<ServiceResponse<List<User>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _context.Users.ToListAsync();
            return new ServiceResponse<List<User>> { Data = users };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<User>> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<User>> GetUserByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new ServiceResponse<User> { Success = false, Message = "User not found." };

            return new ServiceResponse<User> { Data = user };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<User> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<User>> CreateUserAsync(User user)
    {
        try
        {
            // Verificar se username já existe
            var existingUserByUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUserByUsername != null)
            {
                return new ServiceResponse<User> { Success = false, Message = "Username already exists" };
            }

            // Verificar se email já existe
            var existingUserByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUserByEmail != null)
            {
                return new ServiceResponse<User> { Success = false, Message = "Email already exists" };
            }

            user.PasswordHash = _passwordUtils.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new ServiceResponse<User> { Data = user };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<User> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResponse<User>> UpdateUserAsync(int id, User user)
    {
        try
        {
            var findUser = await _context.Users.FindAsync(id);
            if (findUser == null)
            {
                return new ServiceResponse<User> { Success = false, Message = "User not found" };
            }

            findUser.Username = user.Username;
            findUser.Email = user.Email;
            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                findUser.PasswordHash = _passwordUtils.HashPassword(user.PasswordHash);
            }
            findUser.Role = user.Role;
            findUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new ServiceResponse<User> { Data = findUser };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new ServiceResponse<User> { Success = false, Message = "Failed to update the user. " + ex.Message };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<User> { Success = false, Message = "An error occurred while updating the user. " + ex.Message };
        }
    }

    public async Task<ServiceResponse<bool>> DeleteUserAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new ServiceResponse<bool> { Success = false, Message = "User not found." };

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<bool> { Success = false, Message = ex.Message };
        }
    }

}
