using FoodBooking.Application.Features.Payments.DTOs.Requests;
using FoodBooking.Application.Features.Payments.DTOs.Responses;
using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Abstractions;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default);
}
