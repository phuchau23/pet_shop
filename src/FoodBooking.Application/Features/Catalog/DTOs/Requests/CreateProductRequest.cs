namespace FoodBooking.Application.Features.Catalog.DTOs.Requests;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string>? ImageUrls { get; set; } // Danh sách URL ảnh từ Cloudinary (optional)
}