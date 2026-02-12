using FluentValidation;
using FoodBooking.Application.Features.Catalog.DTOs.Requests;

namespace FoodBooking.Application.Features.Catalog.DTOs.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Brand is required");

        // Validate ImageUrls if provided
        RuleForEach(x => x.ImageUrls)
            .NotEmpty().WithMessage("Image URL cannot be empty")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL")
            .When(x => x.ImageUrls != null && x.ImageUrls.Any());
    }
}