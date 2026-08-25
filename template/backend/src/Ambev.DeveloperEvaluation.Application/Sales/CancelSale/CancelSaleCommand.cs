using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Command for cancelling a whole sale. Distinct from deleting a sale, and
/// independent from cancelling an individual item (<see cref="CancelSaleItem.CancelSaleItemCommand"/>).
/// </summary>
public record CancelSaleCommand : IRequest<CancelSaleResult>
{
    public Guid Id { get; }

    public CancelSaleCommand(Guid id)
    {
        Id = id;
    }
}
