using Microsoft.AspNetCore.Mvc;
using TheLibraryOfAlexandria.Services;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    // POST: api/Authentication/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login login)
    {
        var response = await _authenticationService.AuthenticateAsync(login.Username, login.Password);
        if (!response.Success)
            return BadRequest(response.Message);

        var token = response.Data;  // Sends jwt
        return Ok(new { token });
    }

    // Optionally add logout if needed
    // POST: api/Authentication/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Here, we would handle any logout logic such as invalidating tokens, etc.
        return Ok(new { message = "Logout successful" });
    }
}
