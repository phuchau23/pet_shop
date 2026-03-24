using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Shippers.DTOs.Requests;
using FoodBooking.Application.Features.Shippers.DTOs.Responses;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;

namespace FoodBooking.Application.Features.Shippers.Services;

public class ShipperManagementService : IShipperManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;

    public ShipperManagementService(IUserRepository userRepository, IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<ShipperResponse>> GetShippersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Where(u => u.UserRole == UserRole.Shipper).Select(MapToResponse);
    }

    public async Task<ShipperResponse?> GetShipperByIdAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(shipperId, cancellationToken);
        if (user == null || user.UserRole != UserRole.Shipper)
        {
            return null;
        }

        return MapToResponse(user);
    }

    public async Task<ShipperResponse> UpdateShipperStatusAsync(int shipperId, UpdateShipperStatusManagementRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredShipperAsync(shipperId, cancellationToken);
        if (!Enum.TryParse<AccountStatus>(request.Status, true, out var status))
        {
            throw new InvalidOperationException("Invalid status. Use Active, Inactive, or Banned");
        }

        user.AccountStatus = status;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<ShipperResponse> UpdateShipperAvailabilityAsync(int shipperId, UpdateShipperAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredShipperAsync(shipperId, cancellationToken);

        // Reuse account status to represent availability without DB schema changes.
        user.AccountStatus = request.IsAvailable ? AccountStatus.Active : AccountStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToResponse(user);
    }

    public async Task<IEnumerable<ShipperOrderResponse>> GetShipperOrdersAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        _ = await GetRequiredShipperAsync(shipperId, cancellationToken);
        var orders = await _orderRepository.GetByShipperIdAsync(shipperId, null, cancellationToken);
        return orders.Select(o => new ShipperOrderResponse
        {
            OrderId = o.Id,
            Status = o.Status,
            TotalPrice = o.TotalPrice,
            FinalAmount = o.FinalAmount,
            FullAddress = o.FullAddress,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        });
    }

    public async Task<ShipperPerformanceResponse?> GetShipperPerformanceAsync(int shipperId, CancellationToken cancellationToken = default)
    {
        var shipper = await GetShipperByIdAsync(shipperId, cancellationToken);
        if (shipper == null)
        {
            return null;
        }

        var orders = (await _orderRepository.GetByShipperIdAsync(shipperId, null, cancellationToken)).ToList();
        var deliveredOrders = orders.Where(o => o.Status == "delivered").ToList();
        var totalDeliveredRevenue = deliveredOrders.Sum(o => o.FinalAmount ?? o.TotalPrice ?? 0);
        var successRate = orders.Count == 0 ? 0 : (double)deliveredOrders.Count / orders.Count * 100;

        return new ShipperPerformanceResponse
        {
            ShipperId = shipperId,
            TotalAssignedOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == "pending"),
            ConfirmedOrders = orders.Count(o => o.Status == "confirmed"),
            ShippingOrders = orders.Count(o => o.Status == "shipping"),
            DeliveredOrders = deliveredOrders.Count,
            CancelledOrders = orders.Count(o => o.Status == "cancelled"),
            TotalDeliveredRevenue = totalDeliveredRevenue,
            DeliverySuccessRate = Math.Round(successRate, 2)
        };
    }

    private async Task<User> GetRequiredShipperAsync(int shipperId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(shipperId, cancellationToken);
        if (user == null || user.UserRole != UserRole.Shipper)
        {
            throw new KeyNotFoundException($"Shipper with id {shipperId} not found");
        }

        return user;
    }

    private static ShipperResponse MapToResponse(User user)
    {
        return new ShipperResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AccountStatus = user.AccountStatus,
            AccountStatusName = user.AccountStatus.ToString(),
            IsAvailable = user.AccountStatus == AccountStatus.Active,
            LastLogin = user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
