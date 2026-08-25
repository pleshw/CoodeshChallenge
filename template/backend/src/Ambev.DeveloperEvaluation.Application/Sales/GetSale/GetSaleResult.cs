using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Result model for both GetSale (single) and GetSales (list) operations —
/// a sale row always carries the same shape regardless of how it was fetched.
/// </summary>
public class GetSaleResult
{
    public Guid Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public bool IsCancelled { get; set; }

    public DateTime? CancelledAt { get; set; }

    public List<SaleItemResult> Items { get; set; } = [];
}
