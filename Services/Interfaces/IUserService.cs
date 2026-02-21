using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IUserService defines the contract for user account management operations.
/// This service handles user CRUD operations, role-based access control, and filtering for admin panels.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves all users with optional filtering by search term, role, and registration date range.
    /// </summary>
    /// <param name="search">Optional: Keyword search matching against Username and Email (case-insensitive).</param>
    /// <param name="role">Optional: Filter by user role (e.g., "Customer", "Admin", "SuperAdmin").</param>
    /// <param name="createdFrom">Optional: Filter users registered on or after this date (UTC).</param>
    /// <param name="createdTo">Optional: Filter users registered on or before this date (UTC).</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of User objects matching all filter criteria
    /// - Failure: Error message if query fails
    /// </returns>
    /// <remarks>
    /// All parameters are optional and can be combined.
    /// Search is case-insensitive and matches partial strings against username and email.
    /// Useful for admin user management, audit trails, and access control.
    /// </remarks>
    Task<ServiceResponse<List<User>>> GetAllUsersAsync(
        string? search = null,
        string? role = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null
    );

    /// <summary>
    /// Retrieves a single user by ID with all account details.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Complete User object with username, email, role, and timestamps
    /// - Failure: Error message if user not found
    /// </returns>
    Task<ServiceResponse<User>> GetUserByIdAsync(int id);

    /// <summary>
    /// Creates a new user account in the system.
    /// </summary>
    /// <param name="user">The User object containing username, email, and initial role assignment.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created User object with generated ID and timestamps
    /// - Failure: Error message if creation fails (e.g., duplicate username/email, validation errors)
    /// </returns>
    /// <remarks>
    /// Username and Email must be unique across all users.
    /// Password should be provided as plaintext and will be hashed using bcrypt during persistence.
    /// Default role is typically "Customer" unless otherwise specified.
    /// CreatedAt and UpdatedAt timestamps are automatically set to current UTC time.
    /// </remarks>
    Task<ServiceResponse<User>> CreateUserAsync(User user);

    /// <summary>
    /// Updates an existing user account with new information.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="user">The updated User object with new values.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Updated User object
    /// - Failure: Error message if user not found or update fails
    /// </returns>
    /// <remarks>
    /// UpdatedAt timestamp is automatically refreshed to current UTC time.
    /// CreatedAt remains unchanged to preserve account creation date.
    /// Role changes should typically go through authorization checks.
    /// Username and Email uniqueness is validated during updates.
    /// </remarks>
    Task<ServiceResponse<User>> UpdateUserAsync(int id, User user);

    /// <summary>
    /// Deletes a user account from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if deletion succeeded
    /// - Failure: Error message if user not found or deletion fails
    /// </returns>
    /// <remarks>
    /// Deletion cascades to associated ShoppingCart, Orders, UserFavorites, and Payment records.
    /// Consider deactivating users instead of deleting them to preserve order history and audit trails.
    /// Admin users may have restrictions preventing deletion to maintain system integrity.
    /// </remarks>
    Task<ServiceResponse<bool>> DeleteUserAsync(int id);
}
