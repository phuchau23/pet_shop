using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Orders.DTOs.Requests;
using FoodBooking.Application.Features.Orders.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // POST /orders - Tạo đơn hàng mới
        group.MapPost("", async (
            [FromBody] CreateOrderRequest request,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/orders/{result.Id}", 
                    ApiResponse<OrderResponse>.Success(result, "Order created successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<OrderResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreateOrder")
        .Produces<ApiResponse<OrderResponse>>(201)
        .Produces<ApiResponse<OrderResponse>>(400);

        // GET /orders/{id} - Lấy chi tiết đơn hàng
        group.MapGet("/{id:int}", async (
            int id,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.GetByIdAsync(id, cancellationToken);
                if (result == null)
                {
                    return Results.NotFound(ApiResponse<OrderResponse>.Error(404, "Order not found"));
                }
                return Results.Ok(ApiResponse<OrderResponse>.Success(result, "Order retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<OrderResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetOrderById")
        .Produces<ApiResponse<OrderResponse>>(200)
        .Produces<ApiResponse<OrderResponse>>(404);

        // GET /orders?customer_phone=... - Lấy đơn hàng theo số điện thoại
        group.MapGet("", async (
            [FromQuery] string? customer_phone,
            [FromQuery] int? shipper_id,
            [FromQuery] string? status,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                IEnumerable<OrderResponse> result;
                
                if (!string.IsNullOrWhiteSpace(customer_phone))
                {
                    result = await orderService.GetByCustomerPhoneAsync(customer_phone, cancellationToken);
                }
                else if (shipper_id.HasValue)
                {
                    result = await orderService.GetByShipperIdAsync(shipper_id, status, cancellationToken);
                }
                else
                {
                    return Results.BadRequest(ApiResponse<IEnumerable<OrderResponse>>.Error(400, 
                        "Either customer_phone or shipper_id must be provided"));
                }

                return Results.Ok(ApiResponse<IEnumerable<OrderResponse>>.Success(result, "Orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<IEnumerable<OrderResponse>>.Error(400, ex.Message));
            }
        })
        .WithName("GetOrders")
        .Produces<ApiResponse<IEnumerable<OrderResponse>>>(200)
        .Produces<ApiResponse<IEnumerable<OrderResponse>>>(400);

        // PATCH /orders/{id}/status - Cập nhật trạng thái đơn hàng
        group.MapPatch("/{id:int}/status", async (
            int id,
            [FromBody] UpdateOrderStatusRequest request,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.UpdateStatusAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<OrderResponse>.Success(result, "Order status updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<OrderResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<OrderResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<OrderResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateOrderStatus")
        .Produces<ApiResponse<OrderResponse>>(200)
        .Produces<ApiResponse<OrderResponse>>(404)
        .Produces<ApiResponse<OrderResponse>>(400);

        // PATCH /orders/{id}/assign-shipper - Gán shipper cho đơn hàng
        group.MapPatch("/{id:int}/assign-shipper", async (
            int id,
            [FromBody] AssignShipperRequest request,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.AssignShipperAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<OrderResponse>.Success(result, "Shipper assigned successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<OrderResponse>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<OrderResponse>.Error(400, ex.Message));
            }
        })
        .WithName("AssignShipper")
        .Produces<ApiResponse<OrderResponse>>(200)
        .Produces<ApiResponse<OrderResponse>>(404)
        .Produces<ApiResponse<OrderResponse>>(400);
    }
}
