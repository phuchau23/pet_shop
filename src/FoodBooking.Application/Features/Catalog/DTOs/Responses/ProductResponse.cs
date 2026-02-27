namespace FoodBooking.Application.Features.Catalog.DTOs.Responses;

public class ProductResponse
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<ProductSizeResponse> ProductSizes { get; set; } = new(); // Danh sách các size với giá và tồn kho riêng
    public List<ProductImageResponse> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductSizeResponse
{
    public int ProductSizeId { get; set; }
    public string Size { get; set; } = string.Empty; // "1kg", "2kg", "5kg", etc.
    public decimal Price { get; set; } // Giá riêng cho size này
    public int StockQuantity { get; set; } // Số lượng tồn kho riêng cho size này
    public bool IsActive { get; set; }
}

public class ProductImageResponse
{
    public int ProductImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}