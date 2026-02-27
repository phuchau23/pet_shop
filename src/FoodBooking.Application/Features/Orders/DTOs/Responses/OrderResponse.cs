using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Orders.DTOs.Responses;

public class OrderResponse
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? FullAddress { get; set; }
    public double ShopLat { get; set; }
    public double ShopLng { get; set; }
    public string ShopName { get; set; } = "Đại Học FPT University";
    public double? CustomerLat { get; set; }
    public double? CustomerLng { get; set; }
    public int? EstimatedDeliveryMinutes { get; set; }
    public double? EstimatedDistanceMeters { get; set; }
    public int? ShipperId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; } // Tổng tiền trước giảm giá
    public decimal? VoucherDiscount { get; set; } // Số tiền giảm từ voucher
    public decimal? FinalAmount { get; set; } // Tổng tiền cuối cùng (sau giảm giá)
    public string? VoucherCode { get; set; } // Mã voucher đã áp dụng
    public string? Note { get; set; }
    public decimal? DeliveryFee { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}
