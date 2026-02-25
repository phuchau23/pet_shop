using System.Linq;
using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Locations.DTOs.Requests;
using FoodBooking.Application.Features.Locations.DTOs.Responses;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Features.Locations.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;

    public LocationService(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<IEnumerable<ProvinceResponse>> GetAllProvincesAsync(CancellationToken cancellationToken = default)
    {
        var provinces = await _locationRepository.GetAllProvincesAsync(cancellationToken);
        return provinces.Select(MapToProvinceResponse);
    }

    public async Task<IEnumerable<DistrictResponse>> GetDistrictsByProvinceCodeAsync(int provinceCode, CancellationToken cancellationToken = default)
    {
        var districts = await _locationRepository.GetDistrictsByProvinceCodeAsync(provinceCode, cancellationToken);
        return districts.Select(MapToDistrictResponse);
    }

    public async Task<IEnumerable<WardResponse>> GetWardsByDistrictCodeAsync(int districtCode, CancellationToken cancellationToken = default)
    {
        var wards = await _locationRepository.GetWardsByDistrictCodeAsync(districtCode, cancellationToken);
        return wards.Select(MapToWardResponse);
    }

    public async Task<ProvinceResponse> UpdateProvinceCoordinatesAsync(int code, UpdateProvinceCoordinatesRequest request, CancellationToken cancellationToken = default)
    {
        var province = await _locationRepository.GetProvinceByCodeAsync(code, cancellationToken);
        
        if (province == null)
        {
            throw new KeyNotFoundException($"Province with code {code} not found");
        }

        province.Latitude = request.Latitude;
        province.Longitude = request.Longitude;

        await _locationRepository.UpdateProvinceAsync(province, cancellationToken);
        
        return MapToProvinceResponse(province);
    }

    private static ProvinceResponse MapToProvinceResponse(Province province)
    {
        return new ProvinceResponse
        {
            Code = province.Code,
            Name = province.Name,
            Latitude = province.Latitude,
            Longitude = province.Longitude
        };
    }

    private static DistrictResponse MapToDistrictResponse(District district)
    {
        return new DistrictResponse
        {
            Code = district.Code,
            Name = district.Name
        };
    }

    private static WardResponse MapToWardResponse(Ward ward)
    {
        return new WardResponse
        {
            Code = ward.Code,
            Name = ward.Name
        };
    }
}
