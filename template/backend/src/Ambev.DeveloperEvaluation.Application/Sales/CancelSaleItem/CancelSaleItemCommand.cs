using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Command for cancelling a single item within a sale. Independent from
/// cancelling the whole sale.
/// </summary>
public record CancelSaleItemCommand : IRequest<CancelSaleItemResult>
{
    public Guid SaleId { get; }

    public Guid ItemId { get; }

    public CancelSaleItemCommand(Guid saleId, Guid itemId)
    {
        SaleId = saleId;
        ItemId = itemId;
    }
}
