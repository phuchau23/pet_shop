namespace FoodBooking.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    // Địa chỉ giao hàng
    public string AddressDetail { get; set; } = string.Empty;
    public int? WardCode { get; set; }
    public int? DistrictCode { get; set; }
    public int? ProvinceCode { get; set; }
    public string? FullAddress { get; set; }
    public double? CustomerLat { get; set; }
    public double? CustomerLng { get; set; }
    
    // Điểm xuất phát (shop)
    public double ShopLat { get; set; } = 10.8506;
    public double ShopLng { get; set; } = 106.7749;
    
    // Shipper
    public int? ShipperId { get; set; }
    
    // Trạng thái
    public string Status { get; set; } = "pending"; // pending | confirmed | shipping | delivered | cancelled
    
    // Giá và ghi chú
    public decimal? TotalPrice { get; set; }
    public string? Note { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
