namespace FoodBooking.Domain.Entities;

public class District
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Codename { get; set; }
    public string? DivisionType { get; set; }
    public int ProvinceCode { get; set; }

    // Navigation properties
    public Province? Province { get; set; }
    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
