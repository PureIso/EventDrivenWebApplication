using EventDrivenArchitecture.Inventory.Data;
using FluentValidation;

namespace EventDrivenArchitecture.Inventory.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty()
            .WithMessage("Product Name is required.");
        RuleFor(product => product.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Product quantity must be greater than zero.");
    }
}
