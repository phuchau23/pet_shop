using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Abstractions;

public interface ILocationRepository
{
    Task<IEnumerable<Province>> GetAllProvincesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<District>> GetDistrictsByProvinceCodeAsync(int provinceCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ward>> GetWardsByDistrictCodeAsync(int districtCode, CancellationToken cancellationToken = default);
    Task<Province?> GetProvinceByCodeAsync(int code, CancellationToken cancellationToken = default);
    Task UpdateProvinceAsync(Province province, CancellationToken cancellationToken = default);
}
