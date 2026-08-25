namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ReactivateSale;

public class ReactivateSaleResponse
{
    public Guid Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;

    public bool IsCancelled { get; set; }
}
