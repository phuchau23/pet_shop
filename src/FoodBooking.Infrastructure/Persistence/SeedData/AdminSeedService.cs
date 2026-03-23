using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Infrastructure.Persistence.SeedData;

public class AdminSeedService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<AdminSeedService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAdminAsync(CancellationToken cancellationToken = default)
    {
        var anyAdminExists = await _context.Users
            .AnyAsync(u => u.UserRole == UserRole.Admin, cancellationToken);

        if (anyAdminExists)
        {
            _logger.LogInformation("Admin user already exists. Skipping admin seed.");
            return;
        }

        var email = _configuration["AdminSeed:Email"] ?? "admin@foodbooking.com";
        var password = _configuration["AdminSeed:Password"] ?? "Admin@12345";
        var fullName = _configuration["AdminSeed:FullName"] ?? "System Admin";
        var phone = _configuration["AdminSeed:PhoneNumber"];

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (existingUser != null)
        {
            existingUser.UserRole = UserRole.Admin;
            existingUser.AccountStatus = AccountStatus.Active;
            existingUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Existing user {Email} has been promoted to Admin.", email);
            return;
        }

        var adminUser = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            UserRole = UserRole.Admin,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Default admin account created with email: {Email}", email);
    }
}
