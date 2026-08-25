using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.IoC.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

/// <summary>
/// Wires the Redis-backed Sales list cache. The multiplexer is registered
/// with <c>abortConnect=false</c> (set on the connection string) so the app
/// still starts even if Redis is temporarily unreachable — the cache itself
/// fails open, so this is purely a "nice to have", never a hard dependency
/// like Postgres or RabbitMQ.
/// </summary>
public class CachingModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis");

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString!));

        builder.Services.AddScoped<ISalesListCache, RedisSalesListCache>();
    }
}
