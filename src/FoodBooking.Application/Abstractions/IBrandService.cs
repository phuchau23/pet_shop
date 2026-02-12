using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IBrandService
{
    Task<IEnumerable<BrandResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<BrandResponse>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<BrandResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BrandResponse> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default);
    Task<BrandResponse> UpdateAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}