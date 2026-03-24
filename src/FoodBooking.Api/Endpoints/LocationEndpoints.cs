using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Locations.DTOs.Requests;
using FoodBooking.Application.Features.Locations.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class LocationEndpoints
{
    public static void MapLocationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/locations")
            .WithTags("Locations")
            .WithOpenApi();

        group.MapGet("/provinces", async (
            ILocationService locationService,
            CancellationToken cancellationToken) =>
        {
            var result = await locationService.GetAllProvincesAsync(cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<ProvinceResponse>>.Success(result, "Provinces retrieved successfully"));
        })
        .WithName("GetAllProvinces")
        .WithSummary("Get all provinces")
        .WithDescription("Retrieve all provinces available in the location dataset.")
        .Produces<ApiResponse<IEnumerable<ProvinceResponse>>>(200);

        group.MapGet("/districts", async (
            [FromQuery] int province_code,
            ILocationService locationService,
            CancellationToken cancellationToken) =>
        {
            var result = await locationService.GetDistrictsByProvinceCodeAsync(province_code, cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<DistrictResponse>>.Success(result, "Districts retrieved successfully"));
        })
        .WithName("GetDistrictsByProvince")
        .WithSummary("Get districts by province code")
        .WithDescription("Retrieve districts filtered by query parameter province_code.")
        .Produces<ApiResponse<IEnumerable<DistrictResponse>>>(200);

        group.MapGet("/wards", async (
            [FromQuery] int district_code,
            ILocationService locationService,
            CancellationToken cancellationToken) =>
        {
            var result = await locationService.GetWardsByDistrictCodeAsync(district_code, cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<WardResponse>>.Success(result, "Wards retrieved successfully"));
        })
        .WithName("GetWardsByDistrict")
        .WithSummary("Get wards by district code")
        .WithDescription("Retrieve wards filtered by query parameter district_code.")
        .Produces<ApiResponse<IEnumerable<WardResponse>>>(200);

        group.MapPut("/provinces/{code:int}/coordinates", async (
            int code,
            [FromBody] UpdateProvinceCoordinatesRequest request,
            ILocationService locationService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await locationService.UpdateProvinceCoordinatesAsync(code, request, cancellationToken);
                return Results.Ok(ApiResponse<ProvinceResponse>.Success(result, "Province coordinates updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<ProvinceResponse>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<ProvinceResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateProvinceCoordinates")
        .WithSummary("Update province coordinates")
        .WithDescription("Update latitude/longitude of a province by code for routing or fee calculation.")
        .Produces<ApiResponse<ProvinceResponse>>(200)
        .Produces<ApiResponse<ProvinceResponse>>(404)
        .Produces<ApiResponse<ProvinceResponse>>(400);
    }
}
