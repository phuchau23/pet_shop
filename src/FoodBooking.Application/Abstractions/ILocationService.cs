using FoodBooking.Application.Features.Locations.DTOs.Requests;
using FoodBooking.Application.Features.Locations.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface ILocationService
{
    Task<IEnumerable<ProvinceResponse>> GetAllProvincesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DistrictResponse>> GetDistrictsByProvinceCodeAsync(int provinceCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<WardResponse>> GetWardsByDistrictCodeAsync(int districtCode, CancellationToken cancellationToken = default);
    Task<ProvinceResponse> UpdateProvinceCoordinatesAsync(int code, UpdateProvinceCoordinatesRequest request, CancellationToken cancellationToken = default);
}
