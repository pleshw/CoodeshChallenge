namespace Ambev.DeveloperEvaluation.Application.Events;

public class SaleReactivatedEvent
{
    public Guid SaleId { get; }

    public string SaleNumber { get; }

    public DateTime OccurredAt { get; }

    public SaleReactivatedEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}
