using Ambev.DeveloperEvaluation.Application.Events.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Wires Rebus to RabbitMQ for publishing/consuming the Sale domain events
/// (SaleCreated, SaleModified, SaleCancelled, ItemCancelled).
/// </summary>
public class MessagingModuleInitializer : IModuleInitializer
{
    private const string InputQueueName = "sales-events";

    public void Initialize(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("RabbitMq");

        builder.Services.AddRebus(configure => configure
            .Transport(t => t.UseRabbitMq(connectionString, InputQueueName))
            .Routing(r => r.TypeBased())
        );

        builder.Services.AddRebusHandler<SaleCreatedEventHandler>();
        builder.Services.AddRebusHandler<SaleModifiedEventHandler>();
        builder.Services.AddRebusHandler<SaleCancelledEventHandler>();
        builder.Services.AddRebusHandler<ItemCancelledEventHandler>();
    }
}
