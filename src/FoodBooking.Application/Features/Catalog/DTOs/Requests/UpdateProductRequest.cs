namespace FoodBooking.Application.Features.Catalog.DTOs.Requests;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; }
    public List<string>? ImageUrls { get; set; } // Danh sách URL ảnh mới (nếu có sẽ thay thế toàn bộ ảnh cũ)
    public List<string>? AvailableSizes { get; set; } // Danh sách kích thước có sẵn: ["1kg", "2kg", "5kg", "10kg", "20kg"]
}