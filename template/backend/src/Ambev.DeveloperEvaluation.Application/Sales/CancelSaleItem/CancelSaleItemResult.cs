namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemResult
{
    public Guid SaleId { get; set; }

    public Guid ItemId { get; set; }

    public bool IsCancelled { get; set; }

    /// <summary>
    /// The sale's total amount after excluding this now-cancelled item.
    /// </summary>
    public decimal SaleTotalAmount { get; set; }
}
