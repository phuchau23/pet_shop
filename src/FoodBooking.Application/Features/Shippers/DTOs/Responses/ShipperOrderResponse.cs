namespace FoodBooking.Application.Features.Shippers.DTOs.Responses;

public class ShipperOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? FullAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
