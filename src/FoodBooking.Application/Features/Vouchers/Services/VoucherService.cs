using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Features.Vouchers.DTOs.Requests;
using FoodBooking.Application.Features.Vouchers.DTOs.Responses;
using FoodBooking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FoodBooking.Application.Features.Vouchers.Services;

public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly ILogger<VoucherService> _logger;

    public VoucherService(IVoucherRepository voucherRepository, ILogger<VoucherService> logger)
    {
        _voucherRepository = voucherRepository;
        _logger = logger;
    }

    public async Task<VoucherResponse?> ValidateAndGetVoucherAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByCodeAsync(code, cancellationToken);
        
        if (voucher == null)
        {
            return null;
        }

        // Validate voucher
        var now = DateTime.UtcNow;
        
        if (!voucher.IsActive)
        {
            throw new InvalidOperationException("Voucher không còn hoạt động");
        }

        if (voucher.StartDate.HasValue && voucher.StartDate > now)
        {
            throw new InvalidOperationException("Voucher chưa đến thời gian sử dụng");
        }

        if (voucher.EndDate.HasValue && voucher.EndDate < now)
        {
            throw new InvalidOperationException("Voucher đã hết hạn");
        }

        if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
        {
            throw new InvalidOperationException("Voucher đã hết lượt sử dụng");
        }

        if (voucher.MinOrderAmount.HasValue && orderAmount < voucher.MinOrderAmount.Value)
        {
            throw new InvalidOperationException($"Đơn hàng tối thiểu {voucher.MinOrderAmount.Value:N0} VNĐ để sử dụng voucher này");
        }

        return MapToResponse(voucher);
    }

    public async Task<VoucherResponse> CreateAsync(CreateVoucherRequest request, CancellationToken cancellationToken = default)
    {
        // Check if code already exists
        var existing = await _voucherRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Voucher với mã {request.Code} đã tồn tại");
        }

        var voucher = new Voucher
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            UsedCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _voucherRepository.CreateAsync(voucher, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<IEnumerable<VoucherResponse>> GetActiveVouchersAsync(CancellationToken cancellationToken = default)
    {
        var vouchers = await _voucherRepository.GetActiveVouchersAsync(cancellationToken);
        return vouchers.Select(MapToResponse);
    }

    public async Task<VoucherResponse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepository.GetByCodeAsync(code, cancellationToken);
        return voucher == null ? null : MapToResponse(voucher);
    }

    public static decimal CalculateDiscount(Voucher voucher, decimal orderAmount)
    {
        decimal discount = 0;

        if (voucher.DiscountType == "percentage")
        {
            discount = orderAmount * (voucher.DiscountValue / 100);
            if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
            {
                discount = voucher.MaxDiscountAmount.Value;
            }
        }
        else if (voucher.DiscountType == "fixed_amount")
        {
            discount = voucher.DiscountValue;
            if (discount > orderAmount)
            {
                discount = orderAmount; // Không giảm quá tổng tiền
            }
        }

        return Math.Round(discount, 2);
    }

    private static VoucherResponse MapToResponse(Voucher voucher)
    {
        return new VoucherResponse
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            DiscountType = voucher.DiscountType,
            DiscountValue = voucher.DiscountValue,
            MinOrderAmount = voucher.MinOrderAmount,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            UsageLimit = voucher.UsageLimit,
            UsedCount = voucher.UsedCount,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            IsActive = voucher.IsActive,
            CreatedAt = voucher.CreatedAt
        };
    }
}
