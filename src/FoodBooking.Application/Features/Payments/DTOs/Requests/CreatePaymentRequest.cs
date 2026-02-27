using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Payments.DTOs.Requests;

public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}
