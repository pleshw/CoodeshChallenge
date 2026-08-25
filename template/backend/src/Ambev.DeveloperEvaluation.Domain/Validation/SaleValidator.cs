using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty().WithMessage("Sale number is required.");

        RuleFor(sale => sale.SaleDate)
            .NotEqual(default(DateTime)).WithMessage("Sale date is required.");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name cannot be longer than 200 characters.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(sale => sale.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(200).WithMessage("Branch name cannot be longer than 200 characters.");

        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.");

        RuleForEach(sale => sale.Items).SetValidator(new SaleItemValidator());
    }
}
