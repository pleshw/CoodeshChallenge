namespace Ambev.DeveloperEvaluation.Application.Events;

public class SaleCreatedEvent
{
    public Guid SaleId { get; }

    public string SaleNumber { get; }

    public DateTime OccurredAt { get; }

    public SaleCreatedEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}
