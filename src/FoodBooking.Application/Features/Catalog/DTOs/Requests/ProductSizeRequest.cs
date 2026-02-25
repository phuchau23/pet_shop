namespace FoodBooking.Application.Features.Catalog.DTOs.Requests;

public class ProductSizeRequest
{
    public string Size { get; set; } = string.Empty; // "1kg", "2kg", "5kg", etc.
    public decimal Price { get; set; } // Giá riêng cho size này
    public int StockQuantity { get; set; } // Số lượng tồn kho riêng cho size này
    public bool IsActive { get; set; } = true;
}
