namespace FoodBooking.Domain.Entities;

public class Province
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Codename { get; set; }
    public string? DivisionType { get; set; }
    public int? PhoneCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Navigation properties
    public ICollection<District> Districts { get; set; } = new List<District>();
}
