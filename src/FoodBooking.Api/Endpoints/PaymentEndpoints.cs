using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Payments.DTOs.Requests;
using FoodBooking.Application.Features.Payments.DTOs.Responses;
using FoodBooking.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .WithOpenApi();

        // POST /payments - Tạo thanh toán
        group.MapPost("", async (
            [FromBody] CreatePaymentRequest request,
            IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await paymentService.CreatePaymentAsync(request, cancellationToken);
                return Results.Created($"/api/payments/{result.Id}",
                    ApiResponse<PaymentResponse>.Success(result, "Payment created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<PaymentResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<PaymentResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<PaymentResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreatePayment")
        .Produces<ApiResponse<PaymentResponse>>(201)
        .Produces<ApiResponse<PaymentResponse>>(400)
        .Produces<ApiResponse<PaymentResponse>>(404);

        // GET /payments/order/{orderId} - Lấy payment theo order ID
        group.MapGet("/order/{orderId:int}", async (
            int orderId,
            IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await paymentService.GetByOrderIdAsync(orderId, cancellationToken);
                if (result == null)
                {
                    return Results.NotFound(ApiResponse<PaymentResponse>.Error(404, "Payment not found"));
                }
                return Results.Ok(ApiResponse<PaymentResponse>.Success(result, "Payment retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<PaymentResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetPaymentByOrderId")
        .Produces<ApiResponse<PaymentResponse>>(200)
        .Produces<ApiResponse<PaymentResponse>>(404);

        // PATCH /payments/{id}/status - Cập nhật trạng thái thanh toán
        group.MapPatch("/{id:int}/status", async (
            int id,
            [FromBody] UpdatePaymentStatusRequest request,
            IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await paymentService.UpdatePaymentStatusAsync(id, request.Status, cancellationToken);
                return Results.Ok(ApiResponse<PaymentResponse>.Success(result, "Payment status updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<PaymentResponse>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<PaymentResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdatePaymentStatus")
        .Produces<ApiResponse<PaymentResponse>>(200)
        .Produces<ApiResponse<PaymentResponse>>(404)
        .Produces<ApiResponse<PaymentResponse>>(400);

    }
}

public class UpdatePaymentStatusRequest
{
    public PaymentStatus Status { get; set; }
}

