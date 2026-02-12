using FluentValidation;
using FoodBooking.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace FoodBooking.Api.Common;

public static class ValidationExceptionHandler
{
    public static void UseValidationExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                if (exception is ValidationException validationException)
                {
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";

                    var errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    var response = ApiResponse<object>.Error(400, "Validation failed");
                    
                    var jsonResponse = new
                    {
                        code = 400,
                        message = "Validation failed",
                        data = (object?)null,
                        errors = errors
                    };

                    var json = JsonSerializer.Serialize(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await context.Response.WriteAsync(json);
                }
            });
        });
    }
}