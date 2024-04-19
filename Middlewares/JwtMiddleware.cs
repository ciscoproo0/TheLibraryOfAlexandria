using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
            // Extract the JWT token from the Authorization header (if present)
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // If a token is found, validate it and attach the user to the context
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
                var tokenHandler = new JwtSecurityTokenHandler();
                // This key should be the same key used to generate the JWT token
                var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"));
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,  // Not validating the issuer
                    ValidateAudience = true, // Not validating the audience
                    // Set clock skew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Cast the validated token to a JwtSecurityToken object to access its properties
                var jwtToken = (JwtSecurityToken)validatedToken;
                // Extract the user ID from the JWT token claims
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                // Attach user ID to context on successful jwt validation
                context.Items["User"] = userId;
            }
            catch
            {
                // If JWT validation fails, do nothing:
                // User is not attached to context so request won't have access to secure routes
            }
        }
    }
}
