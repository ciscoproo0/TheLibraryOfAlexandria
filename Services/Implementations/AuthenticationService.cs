using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Services;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// AuthenticationService implements user authentication logic with JWT token generation.
/// This service handles credential verification using bcrypt password hashing and issues
/// cryptographically signed JWT tokens for API authentication.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordUtils _passwordUtils;
    private readonly IConfiguration _configuration;
    private readonly string _key;

    /// <summary>
    /// Initializes a new instance of AuthenticationService with required dependencies.
    /// Validates JWT_SECRET_KEY configuration on instantiation.
    /// </summary>
    /// <param name="context">Entity Framework database context for user queries.</param>
    /// <param name="passwordUtils">Utility for bcrypt password hashing and verification.</param>
    /// <param name="configuration">Application configuration for JWT settings (JWT_SECRET_KEY, JWT_ISSUER, JWT_AUDIENCE).</param>
    /// <exception cref="InvalidOperationException">Thrown if JWT_SECRET_KEY is not configured.</exception>
    public AuthenticationService(ApplicationDbContext context, PasswordUtils passwordUtils, IConfiguration configuration)
    {
        _context = context;
        _passwordUtils = passwordUtils;
        _configuration = configuration;
        // Validate JWT_SECRET_KEY is configured; throw if missing to fail fast during startup
        _key = _configuration["JWT_SECRET_KEY"] ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured.");
    }

    /// <summary>
    /// Authenticates a user by their email and password, returning a JWT token on success.
    /// Queries the database for the user by email, verifies the password using bcrypt,
    /// and generates a signed JWT token containing user claims if credentials are valid.
    /// </summary>
    /// <param name="username">Email address of the user (used as username for authentication).</param>
    /// <param name="password">Plaintext password to verify against stored bcrypt hash.</param>
    /// <returns>
    /// ServiceResponse containing JWT token as string if authentication succeeds,
    /// or error message if user not found or password is incorrect.
    /// </returns>
    public async Task<ServiceResponse<string>> AuthenticateAsync(string username, string password)
    {
        // Query user by email (email is used as username)
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == username);
        if (user == null)
            return new ServiceResponse<string> { Success = false, Message = "User not found." };

        // Verify password using bcrypt constant-time comparison
        if (!_passwordUtils.VerifyPassword(password, user.PasswordHash))
            return new ServiceResponse<string> { Success = false, Message = "Password is incorrect." };

        // Generate JWT token with user claims
        var token = GenerateJwtToken(user);
        return new ServiceResponse<string> { Data = token };
    }

    /// <summary>
    /// Generates a JWT token signed with HS256 algorithm containing user identity claims.
    /// Token includes user ID, username, role, and email claims. Expires in 7 days.
    /// Uses JWT_SECRET_KEY for signing and JWT_ISSUER/JWT_AUDIENCE from environment.
    /// </summary>
    /// <param name="user">User object containing ID, username, role, and email to include in token.</param>
    /// <returns>Serialized JWT token as string ready for use in Authorization header.</returns>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);
        // Build token descriptor with user claims
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Include standard claims for authentication and authorization
            Subject = new ClaimsIdentity(new Claim[]
            {
            // NameIdentifier claim is used to extract user ID during request processing
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            // Role claim enables role-based authorization on controllers/endpoints
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
            }),
            // Token valid for 7 days from issuance
            Expires = DateTime.UtcNow.AddDays(7),
            // Sign with HS256 (HMAC with SHA-256)
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            // Optional: Issuer and Audience for token validation scoping
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        };

        // Create and serialize the token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}


