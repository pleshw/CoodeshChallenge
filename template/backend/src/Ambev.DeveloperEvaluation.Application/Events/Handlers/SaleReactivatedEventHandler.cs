using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

public class SaleReactivatedEventHandler : IHandleMessages<SaleReactivatedEvent>
{
    private readonly ILogger<SaleReactivatedEventHandler> _logger;

    public SaleReactivatedEventHandler(ILogger<SaleReactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleReactivatedEvent notification)
    {
        _logger.LogInformation(
            "SaleReactivated: sale {SaleId} ({SaleNumber}) reactivated at {OccurredAt}",
            notification.SaleId, notification.SaleNumber, notification.OccurredAt);

        return Task.CompletedTask;
    }
}
