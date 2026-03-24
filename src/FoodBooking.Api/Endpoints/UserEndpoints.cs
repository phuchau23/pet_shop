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
            [FromQuery] string? status,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await userService.GetAllUsersAsync(status, cancellationToken);
                return Results.Ok(ApiResponse<IEnumerable<UserResponse>>.Success(result, "Users retrieved successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResponse<IEnumerable<UserResponse>>.Error(400, ex.Message));
            }
        })
        .WithName("GetAllUsers")
        .WithSummary("Get all users")
        .WithDescription("Admin endpoint to list all user accounts. Optional query: status=Active|Inactive|Banned.")
        .Produces<ApiResponse<IEnumerable<UserResponse>>>(200)
        .Produces<ApiResponse<IEnumerable<UserResponse>>>(400)
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
        .WithSummary("Get user by id")
        .WithDescription("Authorized endpoint to get a user by numeric id.")
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
        .WithSummary("Create user account")
        .WithDescription("Admin endpoint to create a user and assign initial role.")
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
        .WithSummary("Update user profile")
        .WithDescription("Admin endpoint to update user info such as email, full name, and phone number.")
        .Produces<ApiResponse<UserResponse>>(200)
        .Produces<ApiResponse<UserResponse>>(404)
        .Produces<ApiResponse<UserResponse>>(400);

        group.MapDelete("/{id:int}", [Authorize(Roles = "Admin")] async (
            int id,
            IUserManagementService userService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await userService.DeleteUserAsync(id, cancellationToken);
                return Results.Ok(ApiResponse<UserResponse>.Success(result, "User soft-deleted successfully"));
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
        .WithName("DeleteUser")
        .WithSummary("Soft delete user by id")
        .WithDescription("Admin endpoint to soft-delete a user. Only Active users can be deleted; status is changed from Active to Inactive.")
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
        .WithSummary("Update user account status")
        .WithDescription("Admin endpoint to set user status: Active, Inactive, or Banned.")
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
        .WithSummary("Get available roles")
        .WithDescription("Authorized endpoint to list all supported user roles in the system.")
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
        .WithSummary("Get roles of a user")
        .WithDescription("Authorized endpoint to retrieve the role set of a specific user.")
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
        .WithSummary("Update roles of a user")
        .WithDescription("Admin endpoint to update role assignment for a user.")
        .Produces<ApiResponse<UserRolesResponse>>(200)
        .Produces<ApiResponse<UserRolesResponse>>(404)
        .Produces<ApiResponse<UserRolesResponse>>(400);
    }
}
