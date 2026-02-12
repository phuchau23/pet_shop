using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class ImageEndpoints
{
    public static void MapImageEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/images")
            .WithTags("Images")
            .WithOpenApi();

        group.MapPost("/upload", async (
            [FromForm] IFormFile file,
            IImageService imageService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(ApiResponse<UploadImageResponse>.Error(400, "No file uploaded"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Results.BadRequest(ApiResponse<UploadImageResponse>.Error(400, 
                        $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}"));
                }

                // Validate file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    return Results.BadRequest(ApiResponse<UploadImageResponse>.Error(400, 
                        "File size exceeds maximum allowed size of 10MB"));
                }

                // Upload to Cloudinary (folder is hardcoded to "products")
                using var stream = file.OpenReadStream();
                var uploadResult = await imageService.UploadImageAsync(
                    stream, 
                    file.FileName, 
                    cancellationToken);

                var response = new UploadImageResponse
                {
                    ImageUrl = uploadResult.ImageUrl,
                    PublicId = uploadResult.PublicId,
                    FileName = file.FileName,
                    FileSize = file.Length
                };

                return Results.Ok(ApiResponse<UploadImageResponse>.Success(response, "Image uploaded successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<UploadImageResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UploadImage")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<ApiResponse<UploadImageResponse>>(200)
        .Produces<ApiResponse<UploadImageResponse>>(400)
        .DisableAntiforgery(); // Disable for file uploads

        group.MapDelete("/{publicId}", async (
            string publicId,
            IImageService imageService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await imageService.DeleteImageAsync(publicId, cancellationToken);
                if (result)
                {
                    return Results.Ok(ApiResponse<object?>.Success(default, "Image deleted successfully"));
                }
                return Results.BadRequest(ApiResponse<object>.Error(400, "Failed to delete image"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<object>.Error(400, ex.Message));
            }
        })
        .WithName("DeleteImage")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(400);
    }
}
