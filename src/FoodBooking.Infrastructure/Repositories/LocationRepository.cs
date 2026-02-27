using FoodBooking.Application.Abstractions;
using FoodBooking.Domain.Entities;
using FoodBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBooking.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Province>> GetAllProvincesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Provinces
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<District>> GetDistrictsByProvinceCodeAsync(int provinceCode, CancellationToken cancellationToken = default)
    {
        return await _context.Districts
            .Where(d => d.ProvinceCode == provinceCode)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ward>> GetWardsByDistrictCodeAsync(int districtCode, CancellationToken cancellationToken = default)
    {
        return await _context.Wards
            .Where(w => w.DistrictCode == districtCode)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Province?> GetProvinceByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        return await _context.Provinces
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task UpdateProvinceAsync(Province province, CancellationToken cancellationToken = default)
    {
        _context.Provinces.Update(province);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
