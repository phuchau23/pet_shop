namespace FoodBooking.Application.Features.Orders.DTOs.Requests;

public class EstimateDeliveryRequest
{
    public double CustomerLat { get; set; }
    public double CustomerLng { get; set; }
    public decimal? OrderTotal { get; set; } // Tổng tiền đơn hàng (optional) - để tính free delivery
}
