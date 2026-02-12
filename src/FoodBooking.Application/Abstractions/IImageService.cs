namespace FoodBooking.Application.Abstractions;

public class UploadedImageResult
{
    public string ImageUrl { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}

public interface IImageService
{
    Task<UploadedImageResult> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
