using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Vouchers.DTOs.Requests;
using FoodBooking.Application.Features.Vouchers.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        .WithSummary("Get active vouchers")
        .WithDescription("Public endpoint to list currently active vouchers that are valid by date and usage.")
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
        .WithSummary("Get voucher by code")
        .WithDescription("Public endpoint to retrieve voucher details by voucher code.")
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
        .WithSummary("Validate voucher for order amount")
        .WithDescription("Validate voucher conditions against provided order amount and return voucher detail when valid.")
        .Produces<ApiResponse<VoucherResponse>>(200)
        .Produces<ApiResponse<VoucherResponse>>(400)
        .Produces<ApiResponse<VoucherResponse>>(404);

        // POST /vouchers - Tạo voucher mới
        group.MapPost("", [Authorize] async (
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
        .WithSummary("Create voucher")
        .WithDescription("Authorized endpoint to create a new voucher campaign.")
        .Produces<ApiResponse<VoucherResponse>>(201)
        .Produces<ApiResponse<VoucherResponse>>(400);

        // PUT /vouchers/{id} - Cập nhật voucher
        group.MapPut("/{id:int}", [Authorize(Roles = "Admin")] async (
            int id,
            [FromBody] UpdateVoucherRequest request,
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var voucher = await voucherService.UpdateAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<VoucherResponse>.Success(voucher, "Voucher updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<VoucherResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateVoucher")
        .WithSummary("Update voucher by id")
        .WithDescription("Admin endpoint to update voucher detail by path id.")
        .Produces<ApiResponse<VoucherResponse>>(200)
        .Produces<ApiResponse<VoucherResponse>>(404)
        .Produces<ApiResponse<VoucherResponse>>(400);

        // PUT /vouchers - Hỗ trợ format body có Id
        group.MapPut("", [Authorize(Roles = "Admin")] async (
            [FromBody] UpdateVoucherByBodyRequest request,
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updateRequest = new UpdateVoucherRequest
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
                    IsActive = request.IsActive
                };

                var voucher = await voucherService.UpdateAsync(request.Id, updateRequest, cancellationToken);
                return Results.Ok(ApiResponse<VoucherResponse>.Success(voucher, "Voucher updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<VoucherResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<VoucherResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateVoucherByBody")
        .WithSummary("Update voucher by id in body")
        .WithDescription("Admin endpoint to update voucher detail using id in request body for client compatibility.")
        .Produces<ApiResponse<VoucherResponse>>(200)
        .Produces<ApiResponse<VoucherResponse>>(404)
        .Produces<ApiResponse<VoucherResponse>>(400);

        // DELETE /vouchers/{id} - Xóa voucher
        group.MapDelete("/{id:int}", [Authorize(Roles = "Admin")] async (
            int id,
            IVoucherService voucherService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await voucherService.DeleteAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<object>.Success(null, "Voucher deleted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<object>.Error(404, ex.Message));
            }
        })
        .WithName("DeleteVoucher")
        .WithSummary("Delete voucher by id")
        .WithDescription("Admin endpoint to permanently delete a voucher.")
        .Produces<ApiResponse<object>>(200)
        .Produces<ApiResponse<object>>(404);
    }
}

public class ValidateVoucherRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

public class UpdateVoucherByBodyRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
