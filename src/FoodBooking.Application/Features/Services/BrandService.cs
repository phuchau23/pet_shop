using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;
using FoodBooking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Application.Features.Catalog.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IBrandRepository brandRepository, ILogger<BrandService> logger)
    {
        _brandRepository = brandRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<BrandResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _brandRepository.GetAllAsync(cancellationToken);
        return brands.Select(MapToResponse);
    }

    public async Task<PaginatedResponse<BrandResponse>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _brandRepository.GetPagedAsync(request, cancellationToken);
        return new PaginatedResponse<BrandResponse>
        {
            Data = items.Select(MapToResponse),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<BrandResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
        return brand == null ? null : MapToResponse(brand);
    }

    public async Task<BrandResponse> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
    {
        if (await _brandRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Brand with name '{request.Name}' already exists");
        }

        var brand = new Brand
        {
            Name = request.Name
        };

        var created = await _brandRepository.CreateAsync(brand, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<BrandResponse> UpdateAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with id {id} not found");
        }

        if (brand.Name != request.Name && await _brandRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Brand with name '{request.Name}' already exists");
        }

        brand.Name = request.Name;
        brand.UpdatedAt = DateTime.UtcNow;

        await _brandRepository.UpdateAsync(brand, cancellationToken);
        return MapToResponse(brand);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with id {id} not found");
        }

        await _brandRepository.DeleteAsync(brand, cancellationToken);
    }

    private static BrandResponse MapToResponse(Brand brand)
    {
        return new BrandResponse
        {
            BrandId = brand.BrandId,
            Name = brand.Name,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}