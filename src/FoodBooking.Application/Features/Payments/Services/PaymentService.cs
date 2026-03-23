using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Payments.DTOs.Requests;
using FoodBooking.Application.Features.Payments.DTOs.Responses;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoodBooking.Application.Features.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IVnPayService _vnPayService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IVnPayService vnPayService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _vnPayService = vnPayService;
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

        var payableAmount = order.FinalAmount ?? order.TotalPrice;
        if (payableAmount == null || payableAmount <= 0)
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
            Amount = payableAmount.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Handle payment method specific logic
        switch (request.PaymentMethod)
        {
            case PaymentMethod.COD:
                // COD payment - no additional setup needed, payment will be collected on delivery
                break;
            case PaymentMethod.VNPay:
                payment.TransactionRef = BuildTransactionRef(order.Id);
                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    payment.TransactionRef,
                    payment.Amount,
                    $"Thanh toan don hang #{order.Id}",
                    request.ClientIpAddress ?? "127.0.0.1");
                payment.PaymentMetadata = JsonSerializer.Serialize(new
                {
                    gateway = "VNPay",
                    paymentUrl
                });
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

    public async Task<VnPayCallbackResponse> ProcessVnPayCallbackAsync(IDictionary<string, string> queryParams, CancellationToken cancellationToken = default)
    {
        if (!_vnPayService.ValidateSignature(queryParams))
        {
            throw new InvalidOperationException("Invalid VNPay signature");
        }

        if (!queryParams.TryGetValue("vnp_TxnRef", out var txnRef) || string.IsNullOrWhiteSpace(txnRef))
        {
            throw new InvalidOperationException("VNPay callback missing transaction reference");
        }

        var payment = await _paymentRepository.GetByTransactionRefAsync(txnRef, cancellationToken);
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment not found for transaction {txnRef}");
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            return new VnPayCallbackResponse
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                TransactionRef = payment.TransactionRef ?? txnRef,
                PaymentStatus = payment.Status,
                IsSuccess = true,
                Message = "Payment already confirmed"
            };
        }

        var isSuccess = _vnPayService.IsSuccessResponse(queryParams);
        payment.Status = isSuccess ? PaymentStatus.Paid : PaymentStatus.Failed;
        payment.PaidAt = isSuccess ? DateTime.UtcNow : null;
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        if (isSuccess)
        {
            var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken);
            if (order != null && order.Status == "pending")
            {
                order.Status = "confirmed";
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order, cancellationToken);
            }
        }

        return new VnPayCallbackResponse
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            TransactionRef = payment.TransactionRef ?? txnRef,
            PaymentStatus = payment.Status,
            IsSuccess = isSuccess,
            Message = isSuccess ? "VNPay payment successful" : "VNPay payment failed"
        };
    }

    private static string BuildTransactionRef(int orderId)
    {
        return $"{orderId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
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
                PaymentMethod.VNPay => "VNPay",
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
