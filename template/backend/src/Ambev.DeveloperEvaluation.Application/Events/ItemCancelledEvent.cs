namespace Ambev.DeveloperEvaluation.Application.Events;

public class ItemCancelledEvent
{
    public Guid SaleId { get; }

    public Guid ItemId { get; }

    public Guid ProductId { get; }

    public string ProductName { get; }

    public DateTime OccurredAt { get; }

    public ItemCancelledEvent(Guid saleId, Guid itemId, Guid productId, string productName)
    {
        SaleId = saleId;
        ItemId = itemId;
        ProductId = productId;
        ProductName = productName;
        OccurredAt = DateTime.UtcNow;
    }
}
