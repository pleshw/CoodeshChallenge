using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Common.Pagination;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Command for retrieving a paginated, filtered, ordered list of sales
/// </summary>
public class GetSalesCommand : IRequest<PagedResult<GetSaleResult>>
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public bool? IsCancelled { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? BranchId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Field to order by: "saleDate" (default) or "totalAmount".
    /// </summary>
    public string? OrderBy { get; set; }

    public bool Descending { get; set; }
}
