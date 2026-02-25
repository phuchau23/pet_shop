namespace FoodBooking.Application.Features.Orders.DTOs.Responses;

public class OrderResponse
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? FullAddress { get; set; }
    public double ShopLat { get; set; }
    public double ShopLng { get; set; }
    public double? CustomerLat { get; set; }
    public double? CustomerLng { get; set; }
    public int? ShipperId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}
