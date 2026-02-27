namespace FoodBooking.Application.Features.Orders.DTOs.Requests;

public class UpdateShipperStatusRequest
{
    public int ShipperId { get; set; }
    public string Status { get; set; } = string.Empty; // "shipping" hoặc "delivered"
}
