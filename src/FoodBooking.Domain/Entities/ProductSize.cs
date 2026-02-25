namespace FoodBooking.Domain.Entities;

public class ProductSize
{
    public int ProductSizeId { get; set; }
    public int ProductId { get; set; }
    public string Size { get; set; } = string.Empty; // "1kg", "2kg", "5kg", etc.
    public decimal Price { get; set; } // Giá riêng cho size này
    public int StockQuantity { get; set; } // Số lượng tồn kho riêng cho size này
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Product Product { get; set; } = null!;
}
