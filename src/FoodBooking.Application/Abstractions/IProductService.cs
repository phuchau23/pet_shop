using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ProductResponse>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductResponse>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ProductResponse>> GetPagedByCategoryIdAsync(int categoryId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductResponse>> GetByBrandIdAsync(int brandId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<ProductResponse>> GetPagedByBrandIdAsync(int brandId, PaginationRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> AddImagesAsync(int id, IEnumerable<string> imageUrls, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}