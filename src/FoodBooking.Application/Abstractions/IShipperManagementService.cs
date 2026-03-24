using FoodBooking.Application.Features.Shippers.DTOs.Requests;
using FoodBooking.Application.Features.Shippers.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IShipperManagementService
{
    Task<IEnumerable<ShipperResponse>> GetShippersAsync(CancellationToken cancellationToken = default);
    Task<ShipperResponse?> GetShipperByIdAsync(int shipperId, CancellationToken cancellationToken = default);
    Task<ShipperResponse> UpdateShipperStatusAsync(int shipperId, UpdateShipperStatusManagementRequest request, CancellationToken cancellationToken = default);
    Task<ShipperResponse> UpdateShipperAvailabilityAsync(int shipperId, UpdateShipperAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShipperOrderResponse>> GetShipperOrdersAsync(int shipperId, CancellationToken cancellationToken = default);
    Task<ShipperPerformanceResponse?> GetShipperPerformanceAsync(int shipperId, CancellationToken cancellationToken = default);
}
