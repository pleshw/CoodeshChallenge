using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.ReactivateSale;

public class ReactivateSaleValidator : AbstractValidator<ReactivateSaleCommand>
{
    public ReactivateSaleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}
