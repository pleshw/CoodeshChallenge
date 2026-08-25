using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ambev.DeveloperEvaluation.WebApi.HealthChecks;

/// <summary>
/// Confirms the application can actually reach Postgres.
/// </summary>
public class PostgresHealthCheck : IHealthCheck
{
    private readonly DefaultContext _context;

    public PostgresHealthCheck(DefaultContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Connected to Postgres")
                : HealthCheckResult.Unhealthy("Could not connect to Postgres");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres connectivity check threw an exception", ex);
        }
    }
}
