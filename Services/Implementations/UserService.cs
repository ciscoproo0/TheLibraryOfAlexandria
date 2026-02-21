using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// UserService implements user account management with filtering and role-based operations.
/// Handles user CRUD operations, password hashing with bcrypt, and user discovery for admin panels.
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordUtils _passwordUtils;

    /// <summary>
    /// Initializes UserService with database context and password utilities.
    /// </summary>
    public UserService(ApplicationDbContext context, PasswordUtils passwordUtils)
    {
        _context = context;
        _passwordUtils = passwordUtils;
    }

    /// <summary>
    /// Retrieves all users with optional filtering by search term, role, and creation date range.
    /// Search performs case-insensitive partial matching across username and email fields.
    /// Results ordered by creation date (newest first).
    /// </summary>
    public async Task<ServiceResponse<List<User>>> GetAllUsersAsync(
        string? search = null,
        string? role = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null
    )
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // Perform case-insensitive search across username and email with null safety
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u => (u.Username ?? "").ToLower().Contains(s) || (u.Email ?? "").ToLower().Contains(s));
            }

            // Filter by user role with safe enum parsing
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
            {
                query = query.Where(u => u.Role == parsedRole);
            }

            // Filter by registration date range
            if (createdFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= createdFrom.Value);
            }
            if (createdTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= createdTo.Value);
            }

            query = query.OrderByDescending(u => u.CreatedAt);
            var users = await query.ToListAsync();
            return new ServiceResponse<List<User>> { Data = users };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<User>> { Success = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Retrieves a single user by ID.
    /// </summary>
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

    /// <summary>
    /// Creates a new user account with duplicate checking and password hashing.
    /// Username and Email must be unique in the system.
    /// Password is automatically hashed using bcrypt before persistence.
    /// </summary>
    public async Task<ServiceResponse<User>> CreateUserAsync(User user)
    {
        try
        {
            // Validate username uniqueness
            var existingUserByUsername = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUserByUsername != null)
            {
                return new ServiceResponse<User> { Success = false, Message = "Username already exists" };
            }

            // Validate email uniqueness
            var existingUserByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUserByEmail != null)
            {
                return new ServiceResponse<User> { Success = false, Message = "Email already exists" };
            }

            // Hash plaintext password using bcrypt with workFactor=12
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

    /// <summary>
    /// Updates an existing user account with new information.
    /// Password can be updated; if provided, it's automatically hashed with bcrypt.
    /// Updates UpdatedAt timestamp to current UTC time.
    /// </summary>
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
            // Only update password if a new one is provided
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

    /// <summary>
    /// Deletes a user account and all associated records (cascading deletion).
    /// </summary>
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
