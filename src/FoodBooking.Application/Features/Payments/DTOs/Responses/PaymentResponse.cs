using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Payments.DTOs.Responses;

public class PaymentResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionRef { get; set; }
    public string? PaymentMetadata { get; set; } // JSON string chứa thông tin từ payment gateway (QR code, payment URL, etc.)
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
