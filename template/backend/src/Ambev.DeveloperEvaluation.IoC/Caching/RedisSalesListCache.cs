using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Common.Pagination;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ambev.DeveloperEvaluation.IoC.Caching;

/// <summary>
/// Redis-backed <see cref="ISalesListCache"/>. Every entry's key embeds a
/// generation number read from <see cref="GenerationKey"/>; invalidating
/// just increments that counter, so all previously cached pages become
/// unreachable (and expire via their own TTL) without needing to enumerate
/// or delete them individually.
///
/// Fails open: any Redis error is logged and treated as a cache miss/no-op,
/// so an unreachable cache degrades the list endpoint to "always hits the
/// database" instead of breaking it.
/// </summary>
public class RedisSalesListCache : ISalesListCache
{
    private const string GenerationKey = "sales:list:generation";
    private static readonly TimeSpan EntryTtl = TimeSpan.FromMinutes(5);

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisSalesListCache> _logger;

    public RedisSalesListCache(IConnectionMultiplexer redis, ILogger<RedisSalesListCache> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<PagedResult<GetSaleResult>?> GetAsync(GetSalesCommand query, CancellationToken cancellationToken)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = await BuildKeyAsync(db, query);
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<PagedResult<GetSaleResult>>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sales list cache read failed, falling back to the database");
            return null;
        }
    }

    public async Task SetAsync(GetSalesCommand query, PagedResult<GetSaleResult> result, CancellationToken cancellationToken)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = await BuildKeyAsync(db, query);
            var json = JsonSerializer.Serialize(result);
            await db.StringSetAsync(key, json, EntryTtl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sales list cache write failed; the response was still served correctly, just not cached");
        }
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.StringIncrementAsync(GenerationKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sales list cache invalidation failed; stale cached pages may be served until they expire ({Ttl})", EntryTtl);
        }
    }

    private static async Task<string> BuildKeyAsync(IDatabase db, GetSalesCommand query)
    {
        var generation = await db.StringGetAsync(GenerationKey);
        var generationValue = generation.HasValue ? (long)generation : 0;

        var descriptor = string.Join('|',
            query.PageNumber, query.PageSize, query.IsCancelled,
            query.CustomerId, query.BranchId,
            query.StartDate?.ToString("O"), query.EndDate?.ToString("O"),
            query.OrderBy, query.Descending);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(descriptor)));

        return $"sales:list:v{generationValue}:{hash}";
    }
}
