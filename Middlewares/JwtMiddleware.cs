using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TheLibraryOfAlexandria.Middlewares
{
    // JwtMiddleware intercepts HTTP requests to validate JWT tokens in Authorization headers
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        // Constructor: injects the next request delegate in the middleware pipeline
        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Middleware invoke method that is called for every HTTP request
        public async Task Invoke(HttpContext context)
        {
            // Extract and validate the Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                await _next(context);
                return;
            }

            // Parse the Bearer token from the Authorization header
            var parts = authHeader.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var token = parts[1];
            // If a valid token is found, validate it and attach the user to the context
            if (token != null)
                attachUserToContext(context, token);

            // Call the next middleware in the pipeline
            await _next(context);
        }

        // Validates the JWT token and attaches user information to the HTTP context
        private void attachUserToContext(HttpContext context, string token)
        {
            try
            {
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                if (string.IsNullOrEmpty(secretKey))
                {
                    return;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                // This key should be the same key used to generate the JWT token
                var key = Encoding.UTF8.GetBytes(secretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,  // Validating the issuer
                    ValidateAudience = true, // Validating the audience
                    // Set clock skew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Cast the validated token to a JwtSecurityToken object to access its properties
                var jwtToken = (JwtSecurityToken)validatedToken;
                // Extract the user ID from the JWT token claims using standard ClaimTypes.NameIdentifier
                var idClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if (idClaim == null || !int.TryParse(idClaim.Value, out var userId))
                {
                    return;
                }

                // Attach user ID to context on successful jwt validation
                context.Items["User"] = userId;
            }
            catch (Exception ex)
            {
                // Log validation failure for security audit trail
                Console.WriteLine($"JWT validation failed: {ex.GetType().Name} - {ex.Message}");
                // User is not attached to context so request won't have access to secure routes
            }
        }
    }
}

