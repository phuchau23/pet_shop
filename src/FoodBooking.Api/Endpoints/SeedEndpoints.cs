using FoodBooking.Infrastructure.Persistence.SeedData;
using FoodBooking.Application.Common;
using FoodBooking.Application.Abstractions;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class SeedEndpoints
{
    public static void MapSeedEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/seed")
            .WithTags("Seed Data")
            .WithOpenApi();

        group.MapPost("/locations", async (
            LocationSeedService seedService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await seedService.SeedLocationsAsync(cancellationToken);
                return Results.Ok(ApiResponse<object>.Success(null, "Location data seeded successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<object>.Error(400, $"Error seeding data: {ex.Message}"));
            }
        })
        .WithName("SeedLocations")
        .WithSummary("Seed location master data")
        .WithDescription("Run seeding for provinces/districts/wards from configured data source.")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(400);

        // POST /api/seed/fix-password - Fix password hash cho user (chỉ dùng trong development)
        group.MapPost("/fix-password", async (
            [FromBody] FixPasswordRequest request,
            IUserRepository userRepository,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null)
                {
                    return Results.NotFound(ApiResponse<object>.Error(404, $"User with email {request.Email} not found"));
                }

                // Hash password mới với BCrypt
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await userRepository.UpdateAsync(user, cancellationToken);

                return Results.Ok(ApiResponse<object>.Success(null, $"Password hash updated successfully for {request.Email}"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<object>.Error(400, $"Error fixing password: {ex.Message}"));
            }
        })
        .WithName("FixPassword")
        .WithSummary("Fix password hash for user (Development only)")
        .WithDescription("Update password hash for a user with BCrypt. Use this to fix 'Invalid salt version' error when password was inserted directly to database.")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(404)
        .Produces<ApiResponse<object>>(400);
    }

    public class FixPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
