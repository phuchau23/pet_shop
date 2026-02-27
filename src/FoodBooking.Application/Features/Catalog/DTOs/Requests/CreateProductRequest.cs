namespace FoodBooking.Application.Features.Catalog.DTOs.Requests;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Price và StockQuantity sẽ tính từ ProductSizes
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string>? ImageUrls { get; set; } // Danh sách URL ảnh từ Cloudinary (optional)
    public List<ProductSizeRequest>? ProductSizes { get; set; } // Danh sách các size với giá và tồn kho riêng
}