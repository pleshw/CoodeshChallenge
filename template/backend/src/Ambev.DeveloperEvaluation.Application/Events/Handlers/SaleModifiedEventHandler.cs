using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

public class SaleModifiedEventHandler : IHandleMessages<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent notification)
    {
        _logger.LogInformation(
            "SaleModified: sale {SaleId} ({SaleNumber}) modified at {OccurredAt}",
            notification.SaleId, notification.SaleNumber, notification.OccurredAt);

        return Task.CompletedTask;
    }
}
