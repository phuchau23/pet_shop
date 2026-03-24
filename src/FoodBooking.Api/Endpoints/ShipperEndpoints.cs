using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Shippers.DTOs.Requests;
using FoodBooking.Application.Features.Shippers.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class ShipperEndpoints
{
    public static void MapShipperEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/shippers")
            .WithTags("Shippers")
            .WithOpenApi();

        group.MapGet("", [Authorize(Roles = "Admin")] async (
            IShipperManagementService shipperService,
            CancellationToken cancellationToken) =>
        {
            var result = await shipperService.GetShippersAsync(cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<ShipperResponse>>.Success(result, "Shippers retrieved successfully"));
        })
        .WithName("GetShippers")
        .WithSummary("Get all shippers")
        .WithDescription("Admin endpoint to list all users with Shipper role.")
        .Produces<ApiResponse<IEnumerable<ShipperResponse>>>(200)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:int}", [Authorize(Roles = "Admin")] async (
            int id,
            IShipperManagementService shipperService,
            CancellationToken cancellationToken) =>
        {
            var result = await shipperService.GetShipperByIdAsync(id, cancellationToken);
            return result == null
                ? Results.NotFound(ApiResponse<ShipperResponse>.Error(404, "Shipper not found"))
                : Results.Ok(ApiResponse<ShipperResponse>.Success(result, "Shipper retrieved successfully"));
        })
        .WithName("GetShipperById")
        .WithSummary("Get shipper by id")
        .WithDescription("Admin endpoint to retrieve shipper profile by user id.")
        .Produces<ApiResponse<ShipperResponse>>(200)
        .Produces<ApiResponse<ShipperResponse>>(404);

        group.MapPatch("/{id:int}/status", [Authorize(Roles = "Admin")] async (
            int id,
            [FromBody] UpdateShipperStatusManagementRequest request,
            IShipperManagementService shipperService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await shipperService.UpdateShipperStatusAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<ShipperResponse>.Success(result, "Shipper status updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<ShipperResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<ShipperResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateShipperStatusManagement")
        .WithSummary("Update shipper account status")
        .WithDescription("Admin endpoint to update shipper account status: Active, Inactive, or Banned.")
        .Produces<ApiResponse<ShipperResponse>>(200)
        .Produces<ApiResponse<ShipperResponse>>(404)
        .Produces<ApiResponse<ShipperResponse>>(400);

        group.MapPatch("/{id:int}/availability", [Authorize(Roles = "Admin")] async (
            int id,
            [FromBody] UpdateShipperAvailabilityRequest request,
            IShipperManagementService shipperService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await shipperService.UpdateShipperAvailabilityAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<ShipperResponse>.Success(result, "Shipper availability updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<ShipperResponse>.Error(404, ex.Message));
            }
        })
        .WithName("UpdateShipperAvailability")
        .WithSummary("Update shipper availability")
        .WithDescription("Admin endpoint to set shipper availability state for assignment.")
        .Produces<ApiResponse<ShipperResponse>>(200)
        .Produces<ApiResponse<ShipperResponse>>(404);

        group.MapGet("/{id:int}/orders", [Authorize(Roles = "Admin")] async (
            int id,
            IShipperManagementService shipperService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await shipperService.GetShipperOrdersAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<IEnumerable<ShipperOrderResponse>>.Success(result, "Shipper orders retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<IEnumerable<ShipperOrderResponse>>.Error(404, ex.Message));
            }
        })
        .WithName("GetShipperOrdersManagement")
        .WithSummary("Get orders of a shipper")
        .WithDescription("Admin endpoint to retrieve all orders assigned to a shipper.")
        .Produces<ApiResponse<IEnumerable<ShipperOrderResponse>>>(200)
        .Produces<ApiResponse<IEnumerable<ShipperOrderResponse>>>(404);

        group.MapGet("/{id:int}/performance", [Authorize(Roles = "Admin")] async (
            int id,
            IShipperManagementService shipperService,
            CancellationToken cancellationToken) =>
        {
            var result = await shipperService.GetShipperPerformanceAsync(id, cancellationToken);
            return result == null
                ? Results.NotFound(ApiResponse<ShipperPerformanceResponse>.Error(404, "Shipper not found"))
                : Results.Ok(ApiResponse<ShipperPerformanceResponse>.Success(result, "Shipper performance retrieved successfully"));
        })
        .WithName("GetShipperPerformance")
        .WithSummary("Get shipper performance summary")
        .WithDescription("Admin endpoint to retrieve aggregated shipper metrics like delivered count and success rate.")
        .Produces<ApiResponse<ShipperPerformanceResponse>>(200)
        .Produces<ApiResponse<ShipperPerformanceResponse>>(404);
    }
}
