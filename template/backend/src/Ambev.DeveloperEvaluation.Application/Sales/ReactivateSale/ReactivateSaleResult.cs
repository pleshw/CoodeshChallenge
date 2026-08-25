namespace Ambev.DeveloperEvaluation.Application.Sales.ReactivateSale;

public class ReactivateSaleResult
{
    public Guid Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;

    public bool IsCancelled { get; set; }
}
