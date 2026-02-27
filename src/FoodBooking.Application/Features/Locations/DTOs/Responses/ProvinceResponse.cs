namespace FoodBooking.Application.Features.Locations.DTOs.Responses;

public class ProvinceResponse
{
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
