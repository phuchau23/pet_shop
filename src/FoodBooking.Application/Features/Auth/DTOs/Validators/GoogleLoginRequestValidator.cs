using FluentValidation;
using FoodBooking.Application.Features.Auth.DTOs;

namespace FoodBooking.Application.Features.Auth.DTOs.Validators;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google id token is required");
    }
}