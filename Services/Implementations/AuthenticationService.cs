using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Services;
using TheLibraryOfAlexandria.Utils;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordUtils _passwordUtils;
    private readonly IConfiguration _configuration;
    private readonly string _key;

    public AuthenticationService(ApplicationDbContext context, PasswordUtils passwordUtils, IConfiguration configuration)
    {
        _context = context;
        _passwordUtils = passwordUtils;
        _configuration = configuration;
        _key = _configuration["JWT_SECRET_KEY"] ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured.");
    }

    public async Task<ServiceResponse<string>> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == username);
        if (user == null)
            return new ServiceResponse<string> { Success = false, Message = "User not found." };

        if (!_passwordUtils.VerifyPassword(password, user.PasswordHash))
            return new ServiceResponse<string> { Success = false, Message = "Password is incorrect." };

        var token = GenerateJwtToken(user);
        return new ServiceResponse<string> { Data = token };
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}


