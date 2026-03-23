using System.Text.Json;
using FoodBooking.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Infrastructure.External.Routing;

public class OSRMRoutingService : IRoutingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OSRMRoutingService> _logger;
    private const string DefaultOsrmBaseUrl = "http://router.project-osrm.org/route/v1/driving";

    public OSRMRoutingService(HttpClient httpClient, IConfiguration configuration, ILogger<OSRMRoutingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RouteInfo?> GetRouteInfoAsync(double fromLat, double fromLng, double toLat, double toLng, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _configuration["Routing:OsrmBaseUrl"] ?? DefaultOsrmBaseUrl;
            // Request với overview=full để lấy full geometry, geometries=geojson để lấy coordinates
            var url = $"{baseUrl}/{fromLng},{fromLat};{toLng},{toLat}?overview=full&alternatives=false&steps=false&geometries=geojson";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<OSRMResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Routes == null || result.Routes.Count == 0)
            {
                return null;
            }

            var route = result.Routes[0];
            var distance = route.Distance; // meters
            var duration = route.Duration; // seconds

            // Convert to minutes (estimated delivery time)
            var estimatedMinutes = (int)Math.Ceiling(duration / 60.0);

            // Extract route coordinates from geometry
            List<List<double>>? routeCoordinates = null;
            if (route.Geometry?.Coordinates != null)
            {
                routeCoordinates = route.Geometry.Coordinates;
            }

            return new RouteInfo
            {
                DistanceMeters = distance,
                DurationSeconds = duration,
                EstimatedMinutes = estimatedMinutes,
                RouteCoordinates = routeCoordinates
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OSRM API");
            return null;
        }
    }
}

internal class OSRMResponse
{
    public string Code { get; set; } = string.Empty;
    public List<OSRMRoute> Routes { get; set; } = new();
}

internal class OSRMRoute
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public OSRMGeometry? Geometry { get; set; }
}

internal class OSRMGeometry
{
    public string Type { get; set; } = "LineString";
    public List<List<double>> Coordinates { get; set; } = new(); // [[lng, lat], [lng, lat], ...]
}
