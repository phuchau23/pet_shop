using FoodBooking.Domain.Common;

namespace FoodBooking.Domain.Entities;

public class Product : BaseEntity
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Brand Brand { get; set; } = null!;
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    // Phase B: OrderItems
    // Phase C: Reviews
}