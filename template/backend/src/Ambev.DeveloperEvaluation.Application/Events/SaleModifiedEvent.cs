namespace Ambev.DeveloperEvaluation.Application.Events;

public class SaleModifiedEvent
{
    public Guid SaleId { get; }

    public string SaleNumber { get; }

    public DateTime OccurredAt { get; }

    public SaleModifiedEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}
