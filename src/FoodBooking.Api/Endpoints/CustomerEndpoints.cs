using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Customers.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;

namespace FoodBooking.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .WithOpenApi();

        group.MapGet("", [Authorize(Roles = "Admin")] async (
            ICustomerManagementService customerService,
            CancellationToken cancellationToken) =>
        {
            var result = await customerService.GetCustomersAsync(cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<CustomerResponse>>.Success(result, "Customers retrieved successfully"));
        })
        .WithName("GetCustomers")
        .WithSummary("Get all customers")
        .WithDescription("Admin endpoint to list all users with Customer role.")
        .Produces<ApiResponse<IEnumerable<CustomerResponse>>>(200)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:int}", [Authorize(Roles = "Admin")] async (
            int id,
            ICustomerManagementService customerService,
            CancellationToken cancellationToken) =>
        {
            var result = await customerService.GetCustomerByIdAsync(id, cancellationToken);
            return result == null
                ? Results.NotFound(ApiResponse<CustomerResponse>.Error(404, "Customer not found"))
                : Results.Ok(ApiResponse<CustomerResponse>.Success(result, "Customer retrieved successfully"));
        })
        .WithName("GetCustomerById")
        .WithSummary("Get customer by id")
        .WithDescription("Admin endpoint to retrieve customer profile by user id.")
        .Produces<ApiResponse<CustomerResponse>>(200)
        .Produces<ApiResponse<CustomerResponse>>(404);

        group.MapGet("/{id:int}/orders", [Authorize(Roles = "Admin")] async (
            int id,
            ICustomerManagementService customerService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await customerService.GetCustomerOrdersAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<IEnumerable<CustomerOrderResponse>>.Success(result, "Customer orders retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<IEnumerable<CustomerOrderResponse>>.Error(404, ex.Message));
            }
        })
        .WithName("GetCustomerOrders")
        .WithSummary("Get orders of a customer")
        .WithDescription("Admin endpoint to retrieve all orders associated with the customer.")
        .Produces<ApiResponse<IEnumerable<CustomerOrderResponse>>>(200)
        .Produces<ApiResponse<IEnumerable<CustomerOrderResponse>>>(404);

        group.MapGet("/{id:int}/summary", [Authorize(Roles = "Admin")] async (
            int id,
            ICustomerManagementService customerService,
            CancellationToken cancellationToken) =>
        {
            var result = await customerService.GetCustomerSummaryAsync(id, cancellationToken);
            return result == null
                ? Results.NotFound(ApiResponse<CustomerSummaryResponse>.Error(404, "Customer not found"))
                : Results.Ok(ApiResponse<CustomerSummaryResponse>.Success(result, "Customer summary retrieved successfully"));
        })
        .WithName("GetCustomerSummary")
        .WithSummary("Get customer order summary")
        .WithDescription("Admin endpoint to get aggregated customer metrics such as total orders and total spent.")
        .Produces<ApiResponse<CustomerSummaryResponse>>(200)
        .Produces<ApiResponse<CustomerSummaryResponse>>(404);
    }
}
