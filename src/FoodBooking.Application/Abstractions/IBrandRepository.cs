using FoodBooking.Application.Common;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Application.Abstractions;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Brand>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Brand> Items, int TotalCount)> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<Brand> CreateAsync(Brand brand, CancellationToken cancellationToken = default);
    Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default);
    Task DeleteAsync(Brand brand, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}