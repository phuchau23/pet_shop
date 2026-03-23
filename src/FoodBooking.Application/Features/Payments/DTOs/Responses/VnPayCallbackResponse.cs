using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Payments.DTOs.Responses;

public class VnPayCallbackResponse
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
