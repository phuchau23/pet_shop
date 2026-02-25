namespace FoodBooking.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; // Snapshot tên lúc đặt
    public decimal UnitPrice { get; set; } // Snapshot giá lúc đặt
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    
    // Navigation properties
    public Order? Order { get; set; }
}
