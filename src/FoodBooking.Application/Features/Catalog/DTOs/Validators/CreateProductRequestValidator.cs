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

        // Price và StockQuantity sẽ tính từ ProductSizes, không cần validate ở đây

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

        // Validate ProductSizes - bắt buộc phải có ít nhất 1 size
        RuleFor(x => x.ProductSizes)
            .NotNull().WithMessage("ProductSizes is required")
            .NotEmpty().WithMessage("Product must have at least one size");

        RuleForEach(x => x.ProductSizes)
            .ChildRules(ps =>
            {
                ps.RuleFor(s => s.Size)
                    .NotEmpty().WithMessage("Size is required")
                    .MaximumLength(50).WithMessage("Size must not exceed 50 characters");

                ps.RuleFor(s => s.Price)
                    .GreaterThan(0).WithMessage("Price for each size must be greater than 0");

                ps.RuleFor(s => s.StockQuantity)
                    .GreaterThanOrEqualTo(0).WithMessage("Stock quantity for each size must be greater than or equal to 0");
            });
    }
}