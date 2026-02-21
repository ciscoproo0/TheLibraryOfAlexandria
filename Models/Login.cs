using System.ComponentModel.DataAnnotations;

/// <summary>
/// Login is a data transfer object (DTO) for user authentication requests.
/// Contains username (email) and plaintext password credentials for authentication with the API.
/// This model is used exclusively in the Login endpoint; credentials are never stored in plaintext.
/// </summary>
public class Login
{
    /// <summary>
    /// Username for authentication. Typically the user's email address used as login identifier.
    /// Required field; must be 1-50 characters.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; }

    /// <summary>
    /// Plaintext password for authentication. Compared against bcrypt hash stored in User.Password using constant-time comparison.
    /// Required field; must be 6-100 characters.
    /// Never stored in plaintext; only kept in request body for credential verification.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; }
}
