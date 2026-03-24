using FoodBooking.Application.Abstractions;
using FoodBooking.Application.Common;
using FoodBooking.Application.Features.Users.DTOs.Requests;
using FoodBooking.Application.Features.Users.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodBooking.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        group.MapGet("", [Authorize(Roles = "Admin")] async (
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            var result = await userService.GetAllUsersAsync(cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<UserResponse>>.Success(result, "Users retrieved successfully"));
        })
        .WithName("GetAllUsers")
        .Produces<ApiResponse<IEnumerable<UserResponse>>>(200)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:int}", [Authorize] async (
            int id,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            var result = await userService.GetUserByIdAsync(id, cancellationToken);
            return result == null
                ? Results.NotFound(ApiResponse<UserResponse>.Error(404, "User not found"))
                : Results.Ok(ApiResponse<UserResponse>.Success(result, "User retrieved successfully"));
        })
        .WithName("GetUserById")
        .Produces<ApiResponse<UserResponse>>(200)
        .Produces<ApiResponse<UserResponse>>(404);

        group.MapPost("", [Authorize(Roles = "Admin")] async (
            [FromBody] CreateUserRequest request,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await userService.CreateUserAsync(request, cancellationToken);
                return Results.Created($"/api/users/{result.UserId}", ApiResponse<UserResponse>.Success(result, "User created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<UserResponse>.Error(400, ex.Message));
            }
        })
        .WithName("CreateUser")
        .Produces<ApiResponse<UserResponse>>(201)
        .Produces<ApiResponse<UserResponse>>(400);

        group.MapPut("/{id:int}", [Authorize(Roles = "Admin")] async (
            int id,
            [FromBody] UpdateUserRequest request,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await userService.UpdateUserAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<UserResponse>.Success(result, "User updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<UserResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<UserResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateUser")
        .Produces<ApiResponse<UserResponse>>(200)
        .Produces<ApiResponse<UserResponse>>(404)
        .Produces<ApiResponse<UserResponse>>(400);

        group.MapPatch("/{id:int}/status", [Authorize(Roles = "Admin")] async (
            int id,
            [FromBody] UpdateUserStatusRequest request,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await userService.UpdateUserStatusAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<UserResponse>.Success(result, "User status updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<UserResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<UserResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateUserStatus")
        .Produces<ApiResponse<UserResponse>>(200)
        .Produces<ApiResponse<UserResponse>>(404)
        .Produces<ApiResponse<UserResponse>>(400);

        group.MapGet("/roles", [Authorize] async (
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            var result = await userService.GetRolesAsync(cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<RoleResponse>>.Success(result, "Roles retrieved successfully"));
        })
        .WithName("GetRoles")
        .Produces<ApiResponse<IEnumerable<RoleResponse>>>(200);

        group.MapGet("/{id:int}/roles", [Authorize] async (
            int id,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            var result = await userService.GetUserRolesAsync(id, cancellationToken);
            return result == null
                ? Results.NotFound(ApiResponse<UserRolesResponse>.Error(404, "User not found"))
                : Results.Ok(ApiResponse<UserRolesResponse>.Success(result, "User roles retrieved successfully"));
        })
        .WithName("GetUserRoles")
        .Produces<ApiResponse<UserRolesResponse>>(200)
        .Produces<ApiResponse<UserRolesResponse>>(404);

        group.MapPatch("/{id:int}/roles", [Authorize(Roles = "Admin")] async (
            int id,
            [FromBody] UpdateUserRolesRequest request,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await userService.UpdateUserRolesAsync(id, request, cancellationToken);
                return Results.Ok(ApiResponse<UserRolesResponse>.Success(result, "User roles updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ApiResponse<UserRolesResponse>.Error(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<UserRolesResponse>.Error(400, ex.Message));
            }
        })
        .WithName("UpdateUserRoles")
        .Produces<ApiResponse<UserRolesResponse>>(200)
        .Produces<ApiResponse<UserRolesResponse>>(404)
        .Produces<ApiResponse<UserRolesResponse>>(400);
    }
}
