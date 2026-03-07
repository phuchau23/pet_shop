using FoodBooking.Application.Features.Orders.DTOs.Requests;
using FoodBooking.Application.Features.Orders.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(string customerPhone, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetByCustomerPhoneAsync(string customerPhone, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetByShipperIdAsync(int? shipperId, string? status, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetShipperOrdersAsync(int shipperId, string? status, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetAvailableOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateStatusAsync(int id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> AssignShipperAsync(int id, AssignShipperRequest request, CancellationToken cancellationToken = default);
    Task<EstimateDeliveryResponse> EstimateDeliveryTimeAsync(EstimateDeliveryRequest request, CancellationToken cancellationToken = default);
    Task<OrderTrackingResponse> GetTrackingAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateShipperStatusAsync(int id, UpdateShipperStatusRequest request, CancellationToken cancellationToken = default);
    Task UpdateShipperLocationAsync(int orderId, int shipperId, double lat, double lng, CancellationToken cancellationToken = default);
}
