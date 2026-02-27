using FoodBooking.Infrastructure.Persistence.SeedData;
using FoodBooking.Application.Common;

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
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(400);
    }
}
