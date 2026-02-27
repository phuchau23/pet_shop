namespace FoodBooking.Application.Abstractions;

public interface IRoutingService
{
    Task<RouteInfo?> GetRouteInfoAsync(double fromLat, double fromLng, double toLat, double toLng, CancellationToken cancellationToken = default);
}

public class RouteInfo
{
    public double DistanceMeters { get; set; }
    public double DurationSeconds { get; set; }
    public int EstimatedMinutes { get; set; }
    public List<List<double>>? RouteCoordinates { get; set; } // [[lng, lat], [lng, lat], ...]
    public string? PolylineEncoded { get; set; } // Polyline encoded string (optional)
}
