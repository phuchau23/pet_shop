namespace FoodBooking.Domain.Entities;

public class Ward
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Codename { get; set; }
    public string? DivisionType { get; set; }
    public int DistrictCode { get; set; }

    // Navigation properties
    public District? District { get; set; }
}
