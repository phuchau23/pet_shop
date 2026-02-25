using FoodBooking.Application.Features.Orders.DTOs.Requests;
using FoodBooking.Application.Features.Orders.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetByCustomerPhoneAsync(string customerPhone, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetByShipperIdAsync(int? shipperId, string? status, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> AssignShipperAsync(int id, AssignShipperRequest request, CancellationToken cancellationToken = default);
}
