using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Common.Pagination;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Cache for GET /api/Sales list results. Implementations should invalidate
/// via a generation counter (bump one key on any Sale mutation, so every
/// previously cached page becomes unreachable at once) rather than trying to
/// enumerate and delete individually affected keys — a single mutation can
/// affect many cached page/filter/order combinations at once.
/// </summary>
public interface ISalesListCache
{
    /// <summary>
    /// Returns the cached result for this exact query, or null on a cache
    /// miss (including when the cache is unavailable — callers should treat
    /// a miss and an outage the same way: fall back to the repository).
    /// </summary>
    Task<PagedResult<GetSaleResult>?> GetAsync(GetSalesCommand query, CancellationToken cancellationToken);

    /// <summary>
    /// Caches the result for this exact query. Best-effort — implementations
    /// should swallow cache-backend failures rather than let them fail the request.
    /// </summary>
    Task SetAsync(GetSalesCommand query, PagedResult<GetSaleResult> result, CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates every cached list page. Call after any Sale write
    /// (create, update, cancel, reactivate, cancel item, delete).
    /// </summary>
    Task InvalidateAsync(CancellationToken cancellationToken);
}
