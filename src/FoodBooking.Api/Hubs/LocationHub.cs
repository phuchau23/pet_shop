using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace FoodBooking.Api.Hubs;

[Authorize]
public class LocationHub : Hub
{
    // Customer join group để nhận location updates
    public async Task JoinOrderTracking(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    // Shipper join group để gửi location
    public async Task JoinShipperTracking(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"shipper-order-{orderId}");
    }

    // Leave group khi disconnect
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    // Shipper gửi location update (optional - có thể dùng API endpoint thay vì SignalR method)
    public async Task UpdateShipperLocation(int orderId, double lat, double lng)
    {
        // Broadcast đến customer đang track order này
        await Clients.Group($"order-{orderId}").SendAsync("ShipperLocationUpdated", new
        {
            orderId,
            lat,
            lng,
            timestamp = DateTime.UtcNow
        });
    }

    // Notify khi shipper nhận đơn
    public async Task NotifyShipperAssigned(int orderId, int shipperId, string shipperName)
    {
        await Clients.Group($"order-{orderId}").SendAsync("ShipperAssigned", new
        {
            orderId,
            shipperId,
            shipperName,
            timestamp = DateTime.UtcNow
        });
    }

    // Notify khi order status thay đổi
    public async Task NotifyOrderStatusChanged(int orderId, string status, string statusDisplayName)
    {
        await Clients.Group($"order-{orderId}").SendAsync("OrderStatusChanged", new
        {
            orderId,
            status,
            statusDisplayName,
            timestamp = DateTime.UtcNow
        });
    }
}
