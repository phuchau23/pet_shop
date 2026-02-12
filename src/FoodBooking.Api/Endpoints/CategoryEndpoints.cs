using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FoodBooking.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .WithOpenApi();

        group.MapGet("", async (
            [AsParameters] PaginationRequest request,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            var result = await categoryService.GetPagedAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<PaginatedResponse<CategoryResponse>>.Success(result, "Categories retrieved successfully"));
        })
        .WithName("GetAllCategories")
        .Produces<ApiResponse<PaginatedResponse<CategoryResponse>>>(200);

        group.MapGet("/{id:int}", async (
            int id,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await categoryService.GetByIdAsync(id, cancellationToken);
                if (result == null)
                {
                    return Results.NotFound(ApiResponse<CategoryResponse>.Error(404, "Category not found"));
                }
                return Results.Ok(ApiResponse<CategoryResponse>.Success(result, "Category retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<CategoryResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetCategoryById")
        .Produces<ApiResponse<CategoryResponse>>(200)
        .Produces<ApiResponse<CategoryResponse>>(404);

        group.MapPost("", async (
            [FromBody] CreateCategoryRequest request,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await categoryService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/categories/{result.CategoryId}", 
                    ApiResponse<CategoryResponse>.Success(result, "Category created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<CategoryResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<CategoryResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreateCategory")
        .Produces<ApiResponse<CategoryResponse>>(201)
        .Produces<ApiResponse<CategoryResponse>>(400);

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateCategoryRequest request,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await categoryService.UpdateAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<CategoryResponse>.Success(result, "Category updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<CategoryResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<CategoryResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<CategoryResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateCategory")
        .Produces<ApiResponse<CategoryResponse>>(200)
        .Produces<ApiResponse<CategoryResponse>>(404)
        .Produces<ApiResponse<CategoryResponse>>(400);

        group.MapDelete("/{id:int}", async (
            int id,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await categoryService.DeleteAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<object?>.Success(default, "Category deleted successfully"));
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
        .WithName("DeleteCategory")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(404);
    }
}
