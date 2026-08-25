namespace Ambev.DeveloperEvaluation.Common.Pagination;

/// <summary>
/// Framework-free paginated result, usable from the Application layer without
/// depending on EF Core or any WebApi-specific type.
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
