using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Vouchers.DTOs.Requests;
using FoodBooking.Application.Features.Vouchers.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class VoucherEndpoints
{
    public static void MapVoucherEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vouchers")
            .WithTags("Vouchers")
            .WithOpenApi();

        // GET /vouchers - Lấy danh sách voucher đang hoạt động
        group.MapGet("", async (
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var vouchers = await voucherService.GetActiveVouchersAsync(cancellationToken);
                return Results.Ok(ApiResponse<IEnumerable<VoucherResponse>>.Success(vouchers, "Vouchers retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<IEnumerable<VoucherResponse>>.Error(400, ex.Message));
            }
        })
        .WithName("GetActiveVouchers")
        .Produces<ApiResponse<IEnumerable<VoucherResponse>>>(200);

        // GET /vouchers/{code} - Lấy voucher theo mã
        group.MapGet("/{code}", async (
            string code,
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var voucher = await voucherService.GetByCodeAsync(code, cancellationToken);
                if (voucher == null)
                {
                    return Results.NotFound(ApiResponse<VoucherResponse>.Error(404, "Voucher not found"));
                }
                return Results.Ok(ApiResponse<VoucherResponse>.Success(voucher, "Voucher retrieved successfully"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
        })
        .WithName("GetVoucherByCode")
        .Produces<ApiResponse<VoucherResponse>>(200)
        .Produces<ApiResponse<VoucherResponse>>(404);

        // POST /vouchers/validate - Validate voucher với order amount
        group.MapPost("/validate", async (
            [FromBody] ValidateVoucherRequest request,
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var voucher = await voucherService.ValidateAndGetVoucherAsync(
                    request.Code, 
                    request.OrderAmount, 
                    cancellationToken);
                
                if (voucher == null)
                {
                    return Results.NotFound(ApiResponse<VoucherResponse>.Error(404, "Voucher not found"));
                }
                
                return Results.Ok(ApiResponse<VoucherResponse>.Success(voucher, "Voucher is valid"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
        })
        .WithName("ValidateVoucher")
        .Produces<ApiResponse<VoucherResponse>>(200)
        .Produces<ApiResponse<VoucherResponse>>(400)
        .Produces<ApiResponse<VoucherResponse>>(404);

        // POST /vouchers - Tạo voucher mới (Admin only - có thể thêm authorization sau)
        group.MapPost("", async (
            [FromBody] CreateVoucherRequest request,
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var voucher = await voucherService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/vouchers/{voucher.Code}",
                    ApiResponse<VoucherResponse>.Success(voucher, "Voucher created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreateVoucher")
        .Produces<ApiResponse<VoucherResponse>>(201)
        .Produces<ApiResponse<VoucherResponse>>(400);
    }
}

public class ValidateVoucherRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}
