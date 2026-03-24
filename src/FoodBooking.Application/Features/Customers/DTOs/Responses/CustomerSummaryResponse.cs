namespace FoodBooking.Application.Features.Customers.DTOs.Responses;

public class CustomerSummaryResponse
{
    public int CustomerId { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int ShippingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderAt { get; set; }
}
