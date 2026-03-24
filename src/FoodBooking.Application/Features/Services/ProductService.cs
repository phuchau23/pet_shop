using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;
using FoodBooking.Application.Features.Catalog.DTOs.Responses;
using FoodBooking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Application.Features.Catalog.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(MapToResponse);
    }

    public async Task<PaginatedResponse<ProductResponse>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _productRepository.GetPagedAsync(request, cancellationToken);
        return new PaginatedResponse<ProductResponse>
        {
            Data = items.Select(MapToResponse),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return product == null ? null : MapToResponse(product);
    }

    public async Task<IEnumerable<ProductResponse>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByCategoryIdAsync(categoryId, cancellationToken);
        return products.Select(MapToResponse);
    }

    public async Task<PaginatedResponse<ProductResponse>> GetPagedByCategoryIdAsync(int categoryId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _productRepository.GetPagedByCategoryIdAsync(categoryId, request, cancellationToken);
        return new PaginatedResponse<ProductResponse>
        {
            Data = items.Select(MapToResponse),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<IEnumerable<ProductResponse>> GetByBrandIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetByBrandIdAsync(brandId, cancellationToken);
        return products.Select(MapToResponse);
    }

    public async Task<PaginatedResponse<ProductResponse>> GetPagedByBrandIdAsync(int brandId, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _productRepository.GetPagedByBrandIdAsync(brandId, request, cancellationToken);
        return new PaginatedResponse<ProductResponse>
        {
            Data = items.Select(MapToResponse),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        // Validate Category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {request.CategoryId} not found");
        }

        // Validate Brand exists
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with id {request.BrandId} not found");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = 0, // Sẽ tính từ ProductSizes
            StockQuantity = 0, // Sẽ tính từ ProductSizes
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            IsActive = request.IsActive,
            // Tạo ProductImages từ ImageUrls nếu có
            ProductImages = request.ImageUrls?.Select((url, index) => new ProductImage
            {
                ImageUrl = url,
                IsPrimary = index == 0, // Ảnh đầu tiên là primary
                SortOrder = index + 1
            }).ToList() ?? new List<ProductImage>(),
            // Tạo ProductSizes từ request nếu có
            ProductSizes = request.ProductSizes?.Select(ps => new ProductSize
            {
                Size = ps.Size,
                Price = ps.Price,
                StockQuantity = ps.StockQuantity,
                IsActive = ps.IsActive
            }).ToList() ?? new List<ProductSize>()
        };

        var created = await _productRepository.CreateAsync(product, cancellationToken);
        
        // Reload với ProductImages để trả về đầy đủ
        var productWithImages = await _productRepository.GetByIdWithDetailsAsync(created.ProductId, cancellationToken);
        return MapToResponse(productWithImages!);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Product name is required");
        }

        if (request.CategoryId <= 0 || request.BrandId <= 0)
        {
            throw new InvalidOperationException("CategoryId and BrandId must be greater than 0");
        }

        // Validate Category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with id {request.CategoryId} not found");
        }

        // Validate Brand exists
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
        if (brand == null)
        {
            throw new KeyNotFoundException($"Brand with id {request.BrandId} not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        // Price và StockQuantity sẽ tính từ ProductSizes
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        // Xử lý ProductImages: nếu có ImageUrls mới, thay thế toàn bộ ảnh cũ
        if (request.ImageUrls != null)
        {
            // Xóa ảnh cũ
            product.ProductImages.Clear();
            
            // Thêm ảnh mới
            foreach (var (url, index) in request.ImageUrls.Select((url, index) => (url, index)))
            {
                product.ProductImages.Add(new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = index == 0,
                    SortOrder = index + 1
                });
            }
        }

        // Xử lý ProductSizes: nếu có ProductSizes mới, thay thế toàn bộ size cũ
        if (request.ProductSizes != null)
        {
            var duplicateSizes = request.ProductSizes
                .GroupBy(x => x.Size.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateSizes.Any())
            {
                throw new InvalidOperationException($"Duplicate product sizes are not allowed: {string.Join(", ", duplicateSizes)}");
            }

            // Xóa size cũ
            product.ProductSizes.Clear();
            
            // Thêm size mới
            foreach (var psRequest in request.ProductSizes)
            {
                product.ProductSizes.Add(new ProductSize
                {
                    Size = psRequest.Size,
                    Price = psRequest.Price,
                    StockQuantity = psRequest.StockQuantity,
                    IsActive = psRequest.IsActive
                });
            }
        }

        await _productRepository.UpdateAsync(product, cancellationToken);
        
        // Reload với ProductImages để trả về đầy đủ
        var updatedProduct = await _productRepository.GetByIdWithDetailsAsync(product.ProductId, cancellationToken);
        return MapToResponse(updatedProduct!);
    }

    public async Task<ProductResponse> AddImagesAsync(int id, IEnumerable<string> imageUrls, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        var validUrls = imageUrls
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!validUrls.Any())
        {
            throw new InvalidOperationException("No valid image urls provided");
        }

        var hasPrimary = product.ProductImages.Any(x => x.IsPrimary);
        var sortOrder = product.ProductImages.Any() ? product.ProductImages.Max(x => x.SortOrder) : 0;

        foreach (var url in validUrls)
        {
            sortOrder++;
            product.ProductImages.Add(new ProductImage
            {
                ImageUrl = url,
                IsPrimary = !hasPrimary && sortOrder == 1,
                SortOrder = sortOrder
            });
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product, cancellationToken);
        var updatedProduct = await _productRepository.GetByIdWithDetailsAsync(product.ProductId, cancellationToken);
        return MapToResponse(updatedProduct!);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with id {id} not found");
        }

        await _productRepository.DeleteAsync(product, cancellationToken);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        var activeSizes = product.ProductSizes
            .Where(ps => ps.IsActive)
            .ToList();

        // Tính giá rẻ nhất từ các ProductSizes
        var minPrice = activeSizes.Any() 
            ? activeSizes.Min(ps => ps.Price) 
            : 0;

        // Tính tổng tồn kho từ các ProductSizes
        var totalStock = activeSizes.Sum(ps => ps.StockQuantity);

        return new ProductResponse
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = minPrice, // Giá rẻ nhất từ ProductSizes
            StockQuantity = totalStock, // Tổng tồn kho từ ProductSizes
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name ?? string.Empty,
            IsActive = product.IsActive,
            ProductSizes = product.ProductSizes
                .Select(ps => new ProductSizeResponse
                {
                    ProductSizeId = ps.ProductSizeId,
                    Size = ps.Size,
                    Price = ps.Price,
                    StockQuantity = ps.StockQuantity,
                    IsActive = ps.IsActive
                })
                .ToList(),
            Images = product.ProductImages
                .OrderBy(pi => pi.SortOrder)
                .Select(pi => new ProductImageResponse
                {
                    ProductImageId = pi.ProductImageId,
                    ImageUrl = pi.ImageUrl,
                    IsPrimary = pi.IsPrimary,
                    SortOrder = pi.SortOrder
                })
                .ToList(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}