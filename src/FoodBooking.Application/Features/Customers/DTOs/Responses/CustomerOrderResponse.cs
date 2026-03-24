namespace FoodBooking.Application.Features.Customers.DTOs.Responses;

public class CustomerOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? VoucherCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
