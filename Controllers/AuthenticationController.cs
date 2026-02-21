using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Services;

/// <summary>
/// AuthenticationController handles user login and JWT token generation.
/// Provides public endpoints for authenticating users with email/password credentials
/// and receiving JWT bearer tokens for secure API access.
/// Route: api/Authentication
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Authenticates a user with email and password, returning a JWT bearer token.
    /// This endpoint is public and does not require authorization.
    /// Token expires in 7 days and contains user ID, name, role, and email claims.
    /// </summary>
    /// <param name="login">Login credentials containing email (as username) and password</param>
    /// <returns>JWT token as JSON object: { "token": "eyJ..." } on success, 400 BadRequest on failure</returns>
    /// <remarks>
    /// Status Codes:
    /// - 200 OK: Authentication successful, JWT token returned
    /// - 400 BadRequest: User not found or password incorrect
    /// </remarks>
    // POST: api/Authentication/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login login)
    {
        var response = await _authenticationService.AuthenticateAsync(login.Username, login.Password);
        if (!response.Success)
            return BadRequest(response.Message);

        var token = response.Data;  // JWT token for bearer authentication
        return Ok(new { token });
    }

    /// <summary>
    /// Logout endpoint for client-side token invalidation.
    /// Note: JWT tokens are stateless; actual invalidation would require a token blacklist service.
    /// Clients should discard the token from local storage/cookies.
    /// </summary>
    /// <returns>Success message</returns>
    /// <remarks>
    /// Status Code: 200 OK - Logout acknowledgment (client responsible for discarding token)
    /// </remarks>
    // POST: api/Authentication/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Token invalidation would require external state (blacklist/cache).
        // For now, client discards token from storage.
        return Ok(new { message = "Logout successful" });
    }
}
