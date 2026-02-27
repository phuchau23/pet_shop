using FoodBooking.Domain.Enums;

namespace FoodBooking.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
    public decimal Amount { get; set; }
    public string? TransactionRef { get; set; } // Transaction reference (optional)
    
    // Generic fields để lưu metadata cho các payment gateway khác (JSON format)
    // Ví dụ: {"qrCodeUrl": "...", "paymentUrl": "...", "gatewayOrderId": "..."}
    public string? PaymentMetadata { get; set; } // JSON string để lưu thông tin từ payment gateway
    
    // Timestamps
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Order? Order { get; set; }
}
