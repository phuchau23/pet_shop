namespace FoodBooking.Application.Features.Catalog.DTOs.Requests;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Price và StockQuantity sẽ tính từ ProductSizes
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; }
    public List<string>? ImageUrls { get; set; } // Danh sách URL ảnh mới (nếu có sẽ thay thế toàn bộ ảnh cũ)
    public List<ProductSizeRequest>? ProductSizes { get; set; } // Danh sách các size với giá và tồn kho riêng (nếu có sẽ thay thế toàn bộ size cũ)
}