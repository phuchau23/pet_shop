using System.Linq;
using FoodBooking.Domain.Entities;
using FoodBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodBooking.Infrastructure.Persistence.SeedData;

public class LocationSeedService
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LocationSeedService> _logger;

    public LocationSeedService(
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<LocationSeedService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SeedLocationsAsync(CancellationToken cancellationToken = default)
    {
        // Check if data already exists
        if (await _context.Provinces.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Location data already exists. Skipping seed.");
            return;
        }

        _logger.LogInformation("Starting to seed location data from provinces.open-api.vn...");

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://provinces.open-api.vn/api/");
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Fetch all provinces with districts and wards (depth=3)
            var provinces = await httpClient.GetFromJsonAsync<List<ProvinceApiResponse>>(
                "?depth=3", cancellationToken);

            if (provinces == null || !provinces.Any())
            {
                _logger.LogWarning("No province data received from API.");
                return;
            }

            _logger.LogInformation($"Fetched {provinces.Count} provinces from API.");

            // Convert and save provinces
            var provinceEntities = new List<Province>();
            var districtEntities = new List<District>();
            var wardEntities = new List<Ward>();

            // Create Nominatim client for geocoding
            var nominatimClient = _httpClientFactory.CreateClient();
            nominatimClient.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
            nominatimClient.DefaultRequestHeaders.Add("User-Agent", "FoodBooking-App/1.0");
            nominatimClient.Timeout = TimeSpan.FromMinutes(10);

            int provinceCount = 0;
            int successCount = 0;
            int failedCount = 0;
            
            foreach (var provinceApi in provinces)
            {
                provinceCount++;
                _logger.LogInformation($"Processing province {provinceCount}/{provinces.Count}: {provinceApi.Name}");
                
                // Try multiple address formats for better accuracy
                var addressFormats = new[]
                {
                    $"{provinceApi.Name}, Vietnam",
                    $"{provinceApi.DivisionType} {provinceApi.Name}, Vietnam",
                    $"{provinceApi.Name}, Việt Nam",
                    $"Tỉnh {provinceApi.Name}, Vietnam",
                    $"Thành phố {provinceApi.Name}, Vietnam"
                };

                double? lat = null;
                double? lng = null;
                
                foreach (var addressFormat in addressFormats)
                {
                    var (foundLat, foundLng) = await GetCoordinatesFromOSMAsync(
                        nominatimClient, 
                        addressFormat,
                        cancellationToken);
                    
                    if (foundLat.HasValue && foundLng.HasValue)
                    {
                        lat = foundLat;
                        lng = foundLng;
                        _logger.LogInformation($"✓ Got coordinates for {provinceApi.Name} using format '{addressFormat}': {lat}, {lng}");
                        successCount++;
                        break;
                    }
                    
                    // Small delay between retries
                    await Task.Delay(500, cancellationToken);
                }

                if (!lat.HasValue || !lng.HasValue)
                {
                    _logger.LogWarning($"✗ Could not get coordinates for {provinceApi.Name} after trying {addressFormats.Length} formats");
                    failedCount++;
                }

                var province = new Province
                {
                    Code = provinceApi.Code,
                    Name = provinceApi.Name,
                    Codename = provinceApi.Codename,
                    DivisionType = provinceApi.DivisionType,
                    PhoneCode = provinceApi.PhoneCode,
                    Latitude = lat,
                    Longitude = lng
                };
                provinceEntities.Add(province);

                // Process districts
                if (provinceApi.Districts != null)
                {
                    foreach (var districtApi in provinceApi.Districts)
                    {
                        var district = new District
                        {
                            Code = districtApi.Code,
                            Name = districtApi.Name,
                            Codename = districtApi.Codename,
                            DivisionType = districtApi.DivisionType,
                            ProvinceCode = province.Code
                        };
                        districtEntities.Add(district);

                        // Process wards
                        if (districtApi.Wards != null)
                        {
                            foreach (var wardApi in districtApi.Wards)
                            {
                                var ward = new Ward
                                {
                                    Code = wardApi.Code,
                                    Name = wardApi.Name,
                                    Codename = wardApi.Codename,
                                    DivisionType = wardApi.DivisionType,
                                    DistrictCode = district.Code
                                };
                                wardEntities.Add(ward);
                            }
                        }
                    }
                }

                // Rate limiting: Nominatim requires max 1 request per second
                // Delay 1.2 seconds between provinces to be safe (accounting for retries)
                if (provinceCount < provinces.Count)
                {
                    await Task.Delay(1200, cancellationToken);
                }
            }

            // Log summary
            _logger.LogInformation($"Geocoding summary: {successCount} succeeded, {failedCount} failed out of {provinces.Count} provinces");

            // Bulk insert
            _logger.LogInformation($"Inserting {provinceEntities.Count} provinces, {districtEntities.Count} districts, {wardEntities.Count} wards...");

            await _context.Provinces.AddRangeAsync(provinceEntities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Districts.AddRangeAsync(districtEntities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Wards.AddRangeAsync(wardEntities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Location data seeded successfully!");
            
            // Log provinces without coordinates
            var provincesWithoutCoords = provinceEntities
                .Where(p => !p.Latitude.HasValue || !p.Longitude.HasValue)
                .Select(p => p.Name)
                .ToList();
            
            if (provincesWithoutCoords.Any())
            {
                _logger.LogWarning($"Provinces without coordinates ({provincesWithoutCoords.Count}): {string.Join(", ", provincesWithoutCoords)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding location data.");
            throw;
        }
    }

    private async Task<(double? lat, double? lng)> GetCoordinatesFromOSMAsync(
        HttpClient httpClient,
        string address,
        CancellationToken cancellationToken)
    {
        try
        {
            // Nominatim API endpoint with better parameters
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"search?q={encodedAddress}&format=json&limit=1&countrycodes=vn&addressdetails=1";
            
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                // Log but don't throw - we'll retry with different format
                return (null, null);
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = JsonSerializer.Deserialize<List<NominatimResponse>>(jsonString);

            if (results == null || !results.Any())
            {
                return (null, null);
            }

            var firstResult = results.First();
            
            // Validate coordinates are within Vietnam bounds
            // Vietnam approximate bounds: Lat 8.5-23.5, Lng 102-110
            if (double.TryParse(firstResult.Lat, out var lat) && 
                double.TryParse(firstResult.Lon, out var lng))
            {
                // Validate coordinates are reasonable for Vietnam
                if (lat >= 8.0 && lat <= 24.0 && lng >= 102.0 && lng <= 110.0)
                {
                    return (lat, lng);
                }
                else
                {
                    _logger.LogWarning($"Coordinates out of Vietnam bounds for {address}: {lat}, {lng}");
                    return (null, null);
                }
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            // Log but don't throw - we'll retry with different format
            _logger.LogDebug($"Error getting coordinates from OSM for address: {address} - {ex.Message}");
            return (null, null);
        }
    }
}

// API Response Models
public class ProvinceApiResponse
{
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Codename { get; set; }
    public string? DivisionType { get; set; }
    public int? PhoneCode { get; set; }
    public List<DistrictApiResponse>? Districts { get; set; }
}

public class DistrictApiResponse
{
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Codename { get; set; }
    public string? DivisionType { get; set; }
    public List<WardApiResponse>? Wards { get; set; }
}

public class WardApiResponse
{
    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Codename { get; set; }
    public string? DivisionType { get; set; }
}

// Nominatim API Response Model
public class NominatimResponse
{
    [JsonPropertyName("lat")]
    public string Lat { get; set; } = string.Empty;
    
    [JsonPropertyName("lon")]
    public string Lon { get; set; } = string.Empty;
    
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;
}
