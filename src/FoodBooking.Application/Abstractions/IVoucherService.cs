using FoodBooking.Application.Features.Vouchers.DTOs.Requests;
using FoodBooking.Application.Features.Vouchers.DTOs.Responses;

namespace FoodBooking.Application.Abstractions;

public interface IVoucherService
{
    Task<VoucherResponse?> ValidateAndGetVoucherAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default);
    Task<VoucherResponse> CreateAsync(CreateVoucherRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<VoucherResponse>> GetActiveVouchersAsync(CancellationToken cancellationToken = default);
    Task<VoucherResponse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
