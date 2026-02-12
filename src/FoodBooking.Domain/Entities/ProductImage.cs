namespace FoodBooking.Domain.Entities;

public class ProductImage
{
    public int ProductImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
}