using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleItemInputValidator : AbstractValidator<SaleItemInput>
{
    public SaleItemInputValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(item => item.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot be longer than 200 characters.");

        RuleFor(item => item.Quantity)
            .InclusiveBetween(1, 20).WithMessage("Quantity must be between 1 and 20 identical items.");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
    }
}
