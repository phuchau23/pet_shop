using FluentValidation;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;

namespace FoodBooking.Application.Features.Catalog.DTOs.Validators;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required")
            .MaximumLength(150).WithMessage("Brand name must not exceed 150 characters");
    }
}