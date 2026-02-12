using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace FoodBooking.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        group.MapGet("", async (
            [AsParameters] PaginationRequest request,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            var result = await productService.GetPagedAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<PaginatedResponse<ProductResponse>>.Success(result, "Products retrieved successfully"));
        })
        .WithName("GetAllProducts")
        .Produces<ApiResponse<PaginatedResponse<ProductResponse>>>(200);

        group.MapGet("/{id:int}", async (
            int id,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await productService.GetByIdAsync(id, cancellationToken);
                if (result == null)
                {
                    return Results.NotFound(ApiResponse<ProductResponse>.Error(404, "Product not found"));
                }
                return Results.Ok(ApiResponse<ProductResponse>.Success(result, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetProductById")
        .Produces<ApiResponse<ProductResponse>>(200)
        .Produces<ApiResponse<ProductResponse>>(404);

        group.MapGet("/category/{categoryId:int}", async (
            int categoryId,
            [AsParameters] PaginationRequest request,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            var result = await productService.GetPagedByCategoryIdAsync(categoryId, request, cancellationToken);
            return Results.Ok(ApiResponse<PaginatedResponse<ProductResponse>>.Success(result, "Products retrieved successfully"));
        })
        .WithName("GetProductsByCategory")
        .Produces<ApiResponse<PaginatedResponse<ProductResponse>>>(200);

        group.MapGet("/brand/{brandId:int}", async (
            int brandId,
            [AsParameters] PaginationRequest request,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            var result = await productService.GetPagedByBrandIdAsync(brandId, request, cancellationToken);
            return Results.Ok(ApiResponse<PaginatedResponse<ProductResponse>>.Success(result, "Products retrieved successfully"));
        })
        .WithName("GetProductsByBrand")
        .Produces<ApiResponse<PaginatedResponse<ProductResponse>>>(200);

        group.MapPost("", async (
            [FromBody] CreateProductRequest request,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await productService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/products/{result.ProductId}", 
                    ApiResponse<ProductResponse>.Success(result, "Product created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<ProductResponse>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreateProduct")
        .Produces<ApiResponse<ProductResponse>>(201)
        .Produces<ApiResponse<ProductResponse>>(404)
        .Produces<ApiResponse<ProductResponse>>(400);

        group.MapPut("/{id:int}", async (
            int id,
            [FromBody] UpdateProductRequest request,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await productService.UpdateAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<ProductResponse>.Success(result, "Product updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<ProductResponse>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateProduct")
        .Produces<ApiResponse<ProductResponse>>(200)
        .Produces<ApiResponse<ProductResponse>>(404)
        .Produces<ApiResponse<ProductResponse>>(400);

        group.MapDelete("/{id:int}", async (
            int id,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await productService.DeleteAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<object?>.Success(default, "Product deleted successfully"));
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
        .WithName("DeleteProduct")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(404);
    }
}
