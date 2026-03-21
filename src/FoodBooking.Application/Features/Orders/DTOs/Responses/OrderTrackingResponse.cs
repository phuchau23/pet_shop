namespace FoodBooking.Application.Features.Orders.DTOs.Responses;

public class OrderTrackingResponse
{
    public int OrderId { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
    public string? StatusDescription { get; set; }
    public int? ShipperId { get; set; }
    
    // Location data for map tracking
    public double ShopLat { get; set; }
    public double ShopLng { get; set; }
    public double? CustomerLat { get; set; }
    public double? CustomerLng { get; set; }
    public double? ShipperCurrentLat { get; set; }
    public double? ShipperCurrentLng { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<StatusTimelineItem> Timeline { get; set; } = new();
}

public class StatusTimelineItem
{
    public string Status { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
}
