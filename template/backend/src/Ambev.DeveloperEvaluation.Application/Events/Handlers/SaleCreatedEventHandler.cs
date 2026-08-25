using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Events.Handlers;

/// <summary>
/// Consumes SaleCreated events published to RabbitMQ via Rebus and logs them
/// to the application log.
/// </summary>
public class SaleCreatedEventHandler : IHandleMessages<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification)
    {
        _logger.LogInformation(
            "SaleCreated: sale {SaleId} ({SaleNumber}) created at {OccurredAt}",
            notification.SaleId, notification.SaleNumber, notification.OccurredAt);

        return Task.CompletedTask;
    }
}
