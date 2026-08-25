using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Sale> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isCancelled = null,
        Guid? customerId = null,
        Guid? branchId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? orderBy = null,
        bool descending = false,
        CancellationToken cancellationToken = default);
}
