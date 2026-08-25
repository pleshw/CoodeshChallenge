using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

public class SaleCancelledEventHandler : IHandleMessages<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledEvent notification)
    {
        _logger.LogInformation(
            "SaleCancelled: sale {SaleId} ({SaleNumber}) cancelled at {OccurredAt}",
            notification.SaleId, notification.SaleNumber, notification.OccurredAt);

        return Task.CompletedTask;
    }
}
