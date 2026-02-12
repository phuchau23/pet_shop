using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FoodBooking.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBooking.Infrastructure.External.Cloudinary;

public class CloudinaryImageService : IImageService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryImageService> _logger;

    public CloudinaryImageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageService> logger)
    {
        _logger = logger;
        
        var account = new Account(
            settings.Value.CloudName,
            settings.Value.ApiKey,
            settings.Value.ApiSecret
        );
        
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<UploadedImageResult> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            const string folder = "products"; // Hardcode folder cho products
            
            // Generate unique public ID
            var publicId = $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}";
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                PublicId = publicId,
                Folder = folder,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Image uploaded successfully: {Url}, PublicId: {PublicId}", 
                    uploadResult.SecureUrl, uploadResult.PublicId);
                
                return new UploadedImageResult
                {
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId
                };
            }
            
            throw new Exception($"Failed to upload image: {uploadResult.Error?.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            throw;
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }
}
