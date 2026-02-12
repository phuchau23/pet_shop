using FoodBooking.Domain.Common;

namespace FoodBooking.Domain.Entities;

public class Brand : BaseEntity
{
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}