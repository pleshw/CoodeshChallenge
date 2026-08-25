using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Ambev.DeveloperEvaluation.WebApi.HealthChecks;

/// <summary>
/// Opens a short-lived AMQP connection to confirm RabbitMQ is actually reachable —
/// independent of Rebus's own long-lived connection, so it reflects broker health
/// even if Rebus hasn't needed to reconnect recently.
/// </summary>
public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RabbitMqHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("RabbitMq");
        if (string.IsNullOrWhiteSpace(connectionString))
            return HealthCheckResult.Unhealthy("RabbitMq connection string is not configured");

        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Could not connect to RabbitMQ", ex);
        }
    }
}
