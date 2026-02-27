namespace FoodBooking.Application.Features.Orders.DTOs.Responses;

public class EstimateDeliveryResponse
{
    public double ShopLat { get; set; }
    public double ShopLng { get; set; }
    public string ShopName { get; set; } = "Đại Học FPT University";
    public double CustomerLat { get; set; }
    public double CustomerLng { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public double EstimatedDistanceMeters { get; set; }
    public double EstimatedDistanceKm { get; set; }
    public decimal DeliveryFee { get; set; } // Phí ship (VNĐ)
    public List<List<double>>? RouteCoordinates { get; set; } // [[lng, lat], [lng, lat], ...] để vẽ đường đi
}
