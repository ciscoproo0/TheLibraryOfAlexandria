using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Models;

namespace TheLibraryOfAlexandria.Controllers
{
    /// <summary>
    /// UsersController manages user account lifecycle and administrative operations.
    /// Provides endpoints for creating, retrieving, updating, and deleting user accounts with role-based access control.
    /// Different endpoints have different authorization requirements (Admin, SuperAdmin, or ServiceAccount).
    /// All user passwords are hashed using bcrypt with workFactor=12 for cryptographic security.
    /// Route: api/Users
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all users with optional filtering by search term, role, and registration date range.
        /// Supports text search across username and email fields, filtering by assigned role, and date-based discovery.
        /// </summary>
        /// <param name="search">Optional: Text search term matched against username and email (case-insensitive substring match)</param>
        /// <param name="role">Optional: Filter users by assigned role (e.g., "Customer", "Admin", "ServiceAccount", "SuperAdmin")</param>
        /// <param name="createdFrom">Optional: Filter users registered on or after this date</param>
        /// <param name="createdTo">Optional: Filter users registered on or before this date</param>
        /// <returns>List of users matching all specified filter criteria</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only (read user data).
        /// Search Behavior: Text search performs case-insensitive substring matching against username and email.
        /// Role Filtering: Treats role as enum; invalid role values are ignored safely using TryParse.
        /// Status Codes:
        /// - 200 OK: Users retrieved successfully (may be empty list)
        /// - 404 NotFound: Service error or query parsing failed
        /// </remarks>
        // GET: api/Users
        [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] DateTime? createdFrom,
            [FromQuery] DateTime? createdTo
        )
        {
            var result = await _userService.GetAllUsersAsync(search, role, createdFrom, createdTo);
            if (result.Success)
                return Ok(result.Data);
            return NotFound(result.Message);
        }

        /// <summary>
        /// Retrieves a single user account by ID with complete profile information.
        /// Returns user details excluding the bcrypt password hash (security best practice).
        /// </summary>
        /// <param name="id">User ID to retrieve</param>
        /// <returns>User account with ID, username, email, role, and registration/update timestamps</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin only.
        /// Security: Password hash is never returned in API responses.
        /// Status Codes:
        /// - 200 OK: User found
        /// - 404 NotFound: User does not exist
        /// </remarks>
        // GET: api/Users/5
        [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new user account with unique username and email validation.
        /// Password is automatically hashed using bcrypt with workFactor=12 before storage.
        /// Account starts with assigned role (Customer, Admin, ServiceAccount, or SuperAdmin).
        /// </summary>
        /// <param name="user">User object with Username, Email, Password (plaintext), and Role</param>
        /// <returns>Created user account with generated ID and timestamp</returns>
        /// <remarks>
        /// Authorization: SuperAdmin only (sensitive operation).
        /// Business Rules:
        /// - Username must be unique (case-sensitive check)
        /// - Email must be unique (case-insensitive check)
        /// - Password is plaintext in request body, automatically bcrypt-hashed with workFactor=12
        /// - Password field is excluded from response (never returned)
        /// Validation:
        /// - Username: Required, must not duplicate existing usernames
        /// - Email: Required format, must not duplicate existing emails
        /// - Password: Required, sufficient entropy recommended (12+ characters)
        /// - Role: Optional; defaults to "Customer" if not specified
        /// Status Codes:
        /// - 201 Created: User created successfully, Location header contains GET endpoint
        /// - 400 BadRequest: Duplicate username/email, invalid data, or validation failed
        /// </remarks>
        // POST: api/Users
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var result = await _userService.CreateUserAsync(user);
            if (!result.Success)
                return BadRequest(result.Message);
            return CreatedAtAction("GetUser", new { id = result?.Data?.Id }, result?.Data);
        }

        /// <summary>
        /// Updates an existing user account with selective field updates.
        /// Passwords are only re-hashed if a new plaintext password is provided; otherwise existing hash is preserved.
        /// Supports role changes for administrative use cases.
        /// </summary>
        /// <param name="id">User ID to update</param>
        /// <param name="user">Updated user object with fields to modify (Password, Role, etc.)</param>
        /// <returns>204 NoContent on success</returns>
        /// <remarks>
        /// Authorization: Admin, ServiceAccount, and SuperAdmin (different roles manage different aspects).
        /// Business Rules:
        /// - If Password field is provided: automatically bcrypt-hashed with workFactor=12, then stored
        /// - If Password field is null/empty: existing password hash is preserved
        /// - UpdatedAt timestamp is automatically refreshed on successful update
        /// - CreatedAt timestamp is preserved and never modified
        /// - Role can be changed to adjust user permissions
        /// Validation:
        /// - Route ID must match user.Id
        /// Status Codes:
        /// - 204 NoContent: User updated successfully
        /// - 400 BadRequest: Invalid data, role change rejected, or service error
        /// </remarks>
        // PUT: api/Users/5
        [Authorize(Roles = "Admin, ServiceAccount, SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            var result = await _userService.UpdateUserAsync(id, user);
            if (!result.Success)
                return BadRequest(result.Message);
            return NoContent();
        }

        /// <summary>
        /// Deletes a user account from the system.
        /// This is a hard delete that removes the user and all associated data (orders, favorites, cart, etc.).
        /// Consider soft-deleting (marking as inactive) for business intelligence and audit trail preservation.
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>204 NoContent on success</returns>
        /// <remarks>
        /// Authorization: SuperAdmin only (most restrictive operation).
        /// Important Considerations:
        /// - This is a destructive operation that cannot be undone
        /// - All user data is cascade-deleted: orders, favorites, shopping cart, payments, shipping info
        /// - For audit trails and business intelligence, consider marking active=false instead
        /// - If user has existing orders, deletion may fail depending on foreign key constraints
        /// Status Codes:
        /// - 204 NoContent: User deleted successfully
        /// - 404 NotFound: User does not exist
        /// - 400 BadRequest: Deletion failed (e.g., due to foreign key constraints)
        /// </remarks>
        // DELETE: api/Users/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            return NoContent();
        }
    }
}
