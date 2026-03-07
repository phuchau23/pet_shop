namespace FoodBooking.Application.Features.Orders.DTOs.Requests;

public class UpdateShipperStatusRequest
{
    public int ShipperId { get; set; }
    public string Status { get; set; } = string.Empty; // "shipping" hoặc "delivered"
    
    // Vị trí shipper khi nhận đơn (bắt buộc khi status = "shipping")
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}
