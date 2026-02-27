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
    
    // Điểm xuất phát (shop) - FPT University HCM
    public double ShopLat { get; set; } = 10.841449;
    public double ShopLng { get; set; } = 106.809997;
    
    // Thời gian giao hàng dự kiến (phút)
    public int? EstimatedDeliveryMinutes { get; set; }
    public double? EstimatedDistanceMeters { get; set; }
    
    // Shipper
    public int? ShipperId { get; set; }
    
    // Trạng thái
    public string Status { get; set; } = "pending"; // pending | confirmed | shipping | delivered | cancelled
    
    // Giá và ghi chú
    public decimal? TotalPrice { get; set; } // Tổng tiền trước giảm giá
    public decimal? VoucherDiscount { get; set; } // Số tiền giảm từ voucher
    public decimal? FinalAmount { get; set; } // Tổng tiền cuối cùng (sau giảm giá)
    public string? Note { get; set; }
    
    // Voucher
    public int? VoucherId { get; set; }
    public string? VoucherCode { get; set; } // Snapshot mã voucher lúc đặt
    
    // Payment
    public int? PaymentId { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public Voucher? Voucher { get; set; }
}
