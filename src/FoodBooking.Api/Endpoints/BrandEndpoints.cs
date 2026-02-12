using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FoodBooking.Api.Endpoints;

public static class BrandEndpoints
{
    public static void MapBrandEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/brands")
            .WithTags("Brands")
            .WithOpenApi();

        group.MapGet("", async (
            [AsParameters] PaginationRequest request,
            IBrandService brandService,
            CancellationToken cancellationToken) =>
        {
            var result = await brandService.GetPagedAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<PaginatedResponse<BrandResponse>>.Success(result, "Brands retrieved successfully"));
        })
        .WithName("GetAllBrands")
        .Produces<ApiResponse<PaginatedResponse<BrandResponse>>>(200);

        group.MapGet("/{id:int}", async (
            int id,
            IBrandService brandService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await brandService.GetByIdAsync(id, cancellationToken);
                if (result == null)
                {
                    return Results.NotFound(ApiResponse<BrandResponse>.Error(404, "Brand not found"));
                }
                return Results.Ok(ApiResponse<BrandResponse>.Success(result, "Brand retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<BrandResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetBrandById")
        .Produces<ApiResponse<BrandResponse>>(200)
        .Produces<ApiResponse<BrandResponse>>(404);

        group.MapPost("", async (
            [FromBody] CreateBrandRequest request,
            IBrandService brandService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await brandService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/brands/{result.BrandId}", 
                    ApiResponse<BrandResponse>.Success(result, "Brand created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<BrandResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<BrandResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreateBrand")
        .Produces<ApiResponse<BrandResponse>>(201)
        .Produces<ApiResponse<BrandResponse>>(400);

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateBrandRequest request,
            IBrandService brandService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await brandService.UpdateAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<BrandResponse>.Success(result, "Brand updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<BrandResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<BrandResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<BrandResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateBrand")
        .Produces<ApiResponse<BrandResponse>>(200)
        .Produces<ApiResponse<BrandResponse>>(404)
        .Produces<ApiResponse<BrandResponse>>(400);

        group.MapDelete("/{id:int}", async (
            int id,
            IBrandService brandService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await brandService.DeleteAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<object?>.Success(default, "Brand deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<object>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<object>.Error(400, ex.Message));
            }
        })
        .WithName("DeleteBrand")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(404);
    }
}
