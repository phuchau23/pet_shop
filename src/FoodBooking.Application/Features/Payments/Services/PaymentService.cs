using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Payments.DTOs.Requests;
using FoodBooking.Application.Features.Payments.DTOs.Responses;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Application.Features.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Get order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {request.OrderId} not found");
        }

        if (order.TotalPrice == null || order.TotalPrice <= 0)
        {
            throw new InvalidOperationException("Order total price must be greater than 0");
        }

        // Check if payment already exists
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existingPayment != null)
        {
            return MapToResponse(existingPayment);
        }

        var payment = new Payment
        {
            OrderId = request.OrderId,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Unpaid,
            Amount = order.TotalPrice.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Handle payment method specific logic
        switch (request.PaymentMethod)
        {
            case PaymentMethod.COD:
                // COD payment - no additional setup needed, payment will be collected on delivery
                break;
            
            // TODO: Thêm các payment gateway khác sau
            // case PaymentMethod.ZaloPay:
            //     // Handle ZaloPay payment
            //     break;
            // case PaymentMethod.MoMo:
            //     // Handle MoMo payment
            //     break;
            // etc.
            
            default:
                throw new InvalidOperationException($"Payment method {request.PaymentMethod} is not supported");
        }

        var createdPayment = await _paymentRepository.CreateAsync(payment, cancellationToken);
        
        // Update order with payment ID
        order.PaymentId = createdPayment.Id;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return MapToResponse(createdPayment);
    }

    public async Task<PaymentResponse?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return payment == null ? null : MapToResponse(payment);
    }

    public async Task<PaymentResponse> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment with id {paymentId} not found");
        }

        payment.Status = status;
        if (status == PaymentStatus.Paid)
        {
            payment.PaidAt = DateTime.UtcNow;
        }
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        return MapToResponse(payment);
    }

    private static PaymentResponse MapToResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            PaymentMethod = payment.PaymentMethod,
            PaymentMethodName = payment.PaymentMethod switch
            {
                PaymentMethod.COD => "COD",
                // TODO: Thêm các payment method name khác khi thêm payment gateway mới
                _ => payment.PaymentMethod.ToString()
            },
            Status = payment.Status,
            StatusName = payment.Status switch
            {
                PaymentStatus.Unpaid => "Chưa thanh toán",
                PaymentStatus.Paid => "Đã thanh toán",
                PaymentStatus.Refunded => "Đã hoàn tiền",
                PaymentStatus.Failed => "Thanh toán thất bại",
                _ => "Unknown"
            },
            Amount = payment.Amount,
            TransactionRef = payment.TransactionRef,
            PaymentMetadata = payment.PaymentMetadata,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
