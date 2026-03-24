namespace FoodBooking.Application.Features.Shippers.DTOs.Responses;

public class ShipperPerformanceResponse
{
    public int ShipperId { get; set; }
    public int TotalAssignedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int ShippingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalDeliveredRevenue { get; set; }
    public double DeliverySuccessRate { get; set; }
}
