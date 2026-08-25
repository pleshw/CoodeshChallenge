using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

public class ItemCancelledEventHandler : IHandleMessages<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent notification)
    {
        _logger.LogInformation(
            "ItemCancelled: item {ItemId} (product {ProductName}) on sale {SaleId} cancelled at {OccurredAt}",
            notification.ItemId, notification.ProductName, notification.SaleId, notification.OccurredAt);

        return Task.CompletedTask;
    }
}
