using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ReactivateSale;

/// <summary>
/// Command for reverting a whole-sale cancellation.
/// </summary>
public record ReactivateSaleCommand : IRequest<ReactivateSaleResult>
{
    public Guid Id { get; }

    public ReactivateSaleCommand(Guid id)
    {
        Id = id;
    }
}
