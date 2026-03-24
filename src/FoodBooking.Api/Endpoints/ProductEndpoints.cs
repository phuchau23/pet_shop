using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
        .WithSummary("Get paged products")
        .WithDescription("Public endpoint to retrieve product list with pagination parameters.")
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
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, GetInnermostMessage(ex)));
            }
        })
        .WithName("GetProductById")
        .WithSummary("Get product by id")
        .WithDescription("Public endpoint to retrieve product detail by numeric id.")
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
        .WithSummary("Get products by category")
        .WithDescription("Public endpoint to retrieve paged products filtered by category id.")
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
        .WithSummary("Get products by brand")
        .WithDescription("Public endpoint to retrieve paged products filtered by brand id.")
        .Produces<ApiResponse<PaginatedResponse<ProductResponse>>>(200);

        group.MapPost("", [Authorize] async (
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
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, GetInnermostMessage(ex)));
            }
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Authorized endpoint to create a product. Category and brand must exist.")
        .Produces<ApiResponse<ProductResponse>>(201)
        .Produces<ApiResponse<ProductResponse>>(404)
        .Produces<ApiResponse<ProductResponse>>(400);

        group.MapPut("/{id:int}", [Authorize] async (
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
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, GetInnermostMessage(ex)));
            }
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product")
        .WithDescription("Authorized endpoint to update a product by id.")
        .Produces<ApiResponse<ProductResponse>>(200)
        .Produces<ApiResponse<ProductResponse>>(404)
        .Produces<ApiResponse<ProductResponse>>(400);

        group.MapPost("/{id:int}/images/upload", [Authorize] async (
            int id,
            IFormFileCollection files,
            IImageService imageService,
            IProductService productService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // IFormFileCollection is natively supported in Minimal API — no binding issues.
                var validFiles = files
                    .Where(f => f != null && f.Length > 0)
                    .ToList();

                if (validFiles.Count == 0)
                {
                    return Results.BadRequest(ApiResponse<ProductResponse>.Error(400,
                        "No valid files uploaded. Please choose at least one file."));
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                const long maxFileSize = 10 * 1024 * 1024;
                var uploadedUrls = new List<string>();

                foreach (var file in validFiles)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Results.BadRequest(ApiResponse<ProductResponse>.Error(400,
                            $"Invalid file type '{fileExtension}' for {file.FileName}. Allowed: {string.Join(", ", allowedExtensions)}"));
                    }

                    if (file.Length > maxFileSize)
                    {
                        return Results.BadRequest(ApiResponse<ProductResponse>.Error(400,
                            $"File {file.FileName} exceeds maximum size of 10MB"));
                    }

                    await using var stream = file.OpenReadStream();
                    var uploadResult = await imageService.UploadImageAsync(stream, file.FileName, cancellationToken);
                    uploadedUrls.Add(uploadResult.ImageUrl);
                }

                var product = await productService.AddImagesAsync(id, uploadedUrls, cancellationToken);
                return Results.Ok(ApiResponse<ProductResponse>.Success(product, $"Successfully uploaded {uploadedUrls.Count} image(s)"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<ProductResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, ex.Message));
            }
            catch (DbUpdateException ex)
            {
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, GetInnermostMessage(ex)));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<ProductResponse>.Error(400, GetInnermostMessage(ex)));
            }
        })
        .WithName("UploadProductImages")
        .WithSummary("Upload multiple images for a product")
        .WithDescription("Upload 1 or more images via multipart/form-data. Field name: 'files'. Replaces all existing product images. Allowed: jpg, jpeg, png, gif, webp. Max 10MB each.")
        .Accepts<IFormFileCollection>("multipart/form-data")
        .Produces<ApiResponse<ProductResponse>>(200)
        .Produces<ApiResponse<ProductResponse>>(404)
        .Produces<ApiResponse<ProductResponse>>(400)
        .DisableAntiforgery();

        group.MapDelete("/{id:int}", [Authorize] async (
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
        .WithSummary("Delete a product")
        .WithDescription("Authorized endpoint to delete a product by id.")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(404);
    }

    private static string GetInnermostMessage(Exception ex)
    {
        var current = ex;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }

        return current.Message;
    }
}
