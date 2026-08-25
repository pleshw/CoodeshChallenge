namespace Ambev.DeveloperEvaluation.Application.Events;

public class SaleCancelledEvent
{
    public Guid SaleId { get; }

    public string SaleNumber { get; }

    public DateTime OccurredAt { get; }

    public SaleCancelledEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}
