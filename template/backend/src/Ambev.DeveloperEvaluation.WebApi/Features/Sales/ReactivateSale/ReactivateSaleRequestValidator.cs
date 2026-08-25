using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ReactivateSale;

public class ReactivateSaleRequestValidator : AbstractValidator<ReactivateSaleRequest>
{
    public ReactivateSaleRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Sale ID is required");
    }
}
