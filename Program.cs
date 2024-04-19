using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using TheLibraryOfAlexandria.Data;
using TheLibraryOfAlexandria.Services;
using TheLibraryOfAlexandria.Utils;
using TheLibraryOfAlexandria.Middlewares;
using dotenv.net;


// Load environment variables from .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Log the current environment to the console
// $Env:ASPNETCORE_ENVIRONMENT = "Development Or production
Console.WriteLine($"Current Environment: {builder.Environment.EnvironmentName}");

// Database connection string retrieved from environment variables
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "DefaultConnection";

// Registering services
builder.Services.AddMemoryCache(); // Add memory cache services to the application
builder.Services.AddControllers(); // Adds controllers to the services collection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString) // Configure the context to connect to PostgreSQL database
           .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information) // Log database command details
           .EnableSensitiveDataLogging() // Enables sensitive data in logs
           .EnableDetailedErrors()); // Enables detailed errors

// Configure authentication using JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Validate the server (issuer) that created the token
        ValidateAudience = true, // Validate the recipient of the token is authorized to receive
        ValidateLifetime = true, // Validate the token is not expired
        ValidateIssuerSigningKey = true, // Validate signature of the token
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"), // The issuer (signer) of the token
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"), // The audience of the token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"))) // The key used to validate the token
    };
});

// Authorization policies based on roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("ServiceAccount", policy => policy.RequireRole("ServiceAccount"));
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
});

// Configure cookies for security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Enhances security by restricting access to cookie from client side scripts
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are always sent over HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevents the browser from sending this cookie along with cross-site requests
});

// Rate limiting setup to prevent abuse of API endpoints
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/users",
            Limit = 5,
            Period = "5m"
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/*",
            Limit = 100,
            Period = "1m"
        }
    };
});

builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddHttpContextAccessor();

// Dependency injection for custom services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IUserFavoriteService, UserFavoriteService>();
builder.Services.AddScoped<PasswordUtils>();

// Adds options to convert enums to strings in JSON responses
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger documentation setup
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "The Library Of Alexandria - A Magic:The Gathering project", Version = "v1" });
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline based on environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use developer exception page to show detailed error info
}

app.UseRouting(); // Use routing
app.UseIpRateLimiting();
app.UseExceptionHandler("/Error"); // Use built-in error handler
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseAuthentication(); // Use authentication middleware
app.UseMiddleware<JwtMiddleware>(); // Use custom JWT middleware
app.UseAuthorization(); // Use authorization middleware

app.MapControllers(); // Map controller endpoints

app.UseSwagger(); // Use Swagger for API documentation
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Library Of Alexandria - A Magic:The Gathering project");
});

// Apply secure headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

    await next();
});


app.Run();