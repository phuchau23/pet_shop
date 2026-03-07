using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using FoodBooking.Infrastructure.Persistence;
using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Auth.Services;
using FoodBooking.Infrastructure.External.Email;
using FoodBooking.Infrastructure.External.Cloudinary;
using FoodBooking.Infrastructure.External.Google;
using FoodBooking.Infrastructure.Repositories;
using FoodBooking.Api.Endpoints;
using FoodBooking.Application.Abstractions.Auth;
using FoodBooking.Application.Features.Auth.DTOs.Validators;
using FoodBooking.Application.Features.Catalog.Services;
using FoodBooking.Application.Features.Locations.Services;
using FoodBooking.Application.Features.Orders.Services;
using FoodBooking.Infrastructure.Persistence.SeedData;
using FoodBooking.Infrastructure.External.Routing;
using FoodBooking.Application.Features.Payments.Services;
using FoodBooking.Application.Features.Vouchers.Services;
using FoodBooking.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all interfaces (0.0.0.0) for mobile device access
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // Listen on all interfaces
});

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();


// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();

// Configure Cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IImageService, CloudinaryImageService>();
builder.Services.AddScoped<IGoogleTokenVerifier, GoogleTokenVerifier>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ILocationService, LocationService>();

// Register OSRM Routing Service
builder.Services.AddHttpClient<IRoutingService, OSRMRoutingService>();

// Register Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

// Register Seed Service
builder.Services.AddScoped<LocationSeedService>();

// Configure ProblemDetails for better error responses
builder.Services.AddProblemDetails();

// Configure CORS
// Note: WithOrigins() không hỗ trợ wildcard (*) cho port
// Phải dùng SetIsOriginAllowed() để check dynamic
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow all localhost ports (Flutter thay đổi port liên tục)
            // SetIsOriginAllowed cho phép check origin động thay vì hardcode ports
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin))
                    return false;
                
                try
                {
                    var uri = new Uri(origin);
                    var host = uri.Host.ToLower();
                    
                    // Allow localhost với bất kỳ port nào
                    if (host == "localhost" || host == "127.0.0.1" || host == "::1")
                        return true;
                    
                    // Allow local network IPs (để test trên mobile device)
                    // 192.168.x.x, 10.x.x.x, 172.16.x.x - 172.31.x.x
                    if (host.StartsWith("192.168.") || host.StartsWith("10."))
                        return true;
                    
                    // Check 172.16.0.0 - 172.31.255.255 range
                    if (host.StartsWith("172."))
                    {
                        var parts = host.Split('.');
                        if (parts.Length >= 2 && int.TryParse(parts[1], out var secondOctet))
                        {
                            if (secondOctet >= 16 && secondOctet <= 31)
                                return true;
                        }
                    }
                    
                    return false;
                }
                catch
                {
                    return false;
                }
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            // Production: Only allow specific origins
            policy.WithOrigins(
                    "https://yourdomain.com",
                    "https://www.yourdomain.com"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "FoodBooking API", 
        Version = "v1",
        Description = "Food Booking API with Authentication"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token, or just paste the token (will auto-add Bearer prefix)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add SignalR
builder.Services.AddSignalR();

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] 
    ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "FoodBooking",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "FoodBooking",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        // Map role claims correctly
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
    
    // Xử lý token khi thiếu "Bearer" prefix hoặc lấy từ query string
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR: Lấy token từ query string (WebSocket không có header)
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
                return Task.CompletedTask;
            }
            
            // REST API: Lấy token từ header
            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            
            // Nếu không có trong header, thử lấy từ query string
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Query["access_token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                    return Task.CompletedTask;
                }
            }
            else
            {
                // Nếu token không có "Bearer " prefix, tự động thêm vào
                if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = token;
                }
                else
                {
                    // Nếu đã có "Bearer ", extract token
                    context.Token = token.Substring(7);
                }
            }
            
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Đảm bảo role claim được thêm vào ClaimsPrincipal
            var roleClaim = context.Principal?.FindFirst(ClaimTypes.Role);
            if (roleClaim != null)
            {
                var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity != null && !identity.HasClaim(ClaimTypes.Role, roleClaim.Value))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // Log lỗi để debug
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT Authentication failed");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Register HttpClient
builder.Services.AddHttpClient();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();

// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Use ProblemDetails middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

// Auto-migrate database
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "true")
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migration completed successfully.");

        // Auto-seed location data if enabled and not exists
        if (Environment.GetEnvironmentVariable("AUTO_SEED_LOCATIONS") == "true")
        {
            var seedService = scope.ServiceProvider.GetRequiredService<LocationSeedService>();
            await seedService.SeedLocationsAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodBooking API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Enable CORS (must be before UseAuthentication and UseAuthorization)
app.UseCors("AllowFlutterWeb");

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<LocationHub>("/hubs/location");

// Map Endpoints
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapBrandEndpoints();
app.MapProductEndpoints();
app.MapImageEndpoints();
app.MapLocationEndpoints();
app.MapSeedEndpoints();
app.MapOrderEndpoints();
app.MapPaymentEndpoints();
app.MapVoucherEndpoints();

// Health check endpoint
app.MapGet("/", () => "FoodBooking API is running!");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();