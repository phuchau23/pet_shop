using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Orders.DTOs.Requests;
using FoodBooking.Application.Features.Orders.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodBooking.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // POST /orders/estimate-delivery - Tính thời gian giao hàng dự kiến
        group.MapPost("/estimate-delivery", async (
            [FromBody] EstimateDeliveryRequest request,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.EstimateDeliveryTimeAsync(request, cancellationToken);
                return Results.Ok(ApiResponse<EstimateDeliveryResponse>.Success(result, "Delivery time estimated successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<EstimateDeliveryResponse>.Error(400, ex.Message));
            }
        })
        .WithName("EstimateDelivery")
        .Produces<ApiResponse<EstimateDeliveryResponse>>(200)
        .Produces<ApiResponse<EstimateDeliveryResponse>>(400);

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

        // GET /orders - Admin: Lấy tất cả đơn hàng
        group.MapGet("", [Authorize(Roles = "Admin")] async (
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.GetAllOrdersAsync(cancellationToken);
                return Results.Ok(ApiResponse<IEnumerable<OrderResponse>>.Success(result, "All orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<IEnumerable<OrderResponse>>.Error(400, ex.Message));
            }
        })
        .WithName("GetAllOrders")
        .Produces<ApiResponse<IEnumerable<OrderResponse>>>(200)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<ApiResponse<IEnumerable<OrderResponse>>>(400);

        // GET /orders/my-orders - User: Lấy đơn hàng của user hiện tại (từ token)
        group.MapGet("/my-orders", [Authorize] async (
            ClaimsPrincipal user,
            IOrderService orderService,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Lấy userId từ token
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? user.FindFirst("UserId")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                // Lấy profile để lấy phone number
                var profile = await authService.GetProfileAsync(userId, cancellationToken);
                
                if (string.IsNullOrWhiteSpace(profile.PhoneNumber))
                {
                    return Results.BadRequest(ApiResponse<IEnumerable<OrderResponse>>.Error(400, 
                        "Phone number not found in profile. Please update your profile."));
                }

                var result = await orderService.GetMyOrdersAsync(profile.PhoneNumber, cancellationToken);
                return Results.Ok(ApiResponse<IEnumerable<OrderResponse>>.Success(result, "My orders retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<IEnumerable<OrderResponse>>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<IEnumerable<OrderResponse>>.Error(400, ex.Message));
            }
        })
        .WithName("GetMyOrders")
        .Produces<ApiResponse<IEnumerable<OrderResponse>>>(200)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<ApiResponse<IEnumerable<OrderResponse>>>(404)
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

        // GET /orders/{id}/tracking - Lấy thông tin tracking đơn hàng
        group.MapGet("/{id:int}/tracking", async (
            int id,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.GetTrackingAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<OrderTrackingResponse>.Success(result, "Order tracking retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<OrderTrackingResponse>.Error(404, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<OrderTrackingResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetOrderTracking")
        .Produces<ApiResponse<OrderTrackingResponse>>(200)
        .Produces<ApiResponse<OrderTrackingResponse>>(404)
        .Produces<ApiResponse<OrderTrackingResponse>>(400);

        // PATCH /orders/{id}/shipper-status - Shipper cập nhật trạng thái đơn hàng
        group.MapPatch("/{id:int}/shipper-status", async (
            int id,
            [FromBody] UpdateShipperStatusRequest request,
            IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await orderService.UpdateShipperStatusAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<OrderResponse>.Success(result, "Order status updated successfully by shipper"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<OrderResponse>.Error(404, ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
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
        .WithName("UpdateShipperStatus")
        .Produces<ApiResponse<OrderResponse>>(200)
        .Produces<ApiResponse<OrderResponse>>(404)
        .Produces<ApiResponse<OrderResponse>>(401)
        .Produces<ApiResponse<OrderResponse>>(400);
    }
}
