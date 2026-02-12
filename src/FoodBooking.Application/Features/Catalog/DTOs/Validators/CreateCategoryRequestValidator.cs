using FluentValidation;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;

namespace FoodBooking.Application.Features.Catalog.DTOs.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(150).WithMessage("Category name must not exceed 150 characters");
    }
}