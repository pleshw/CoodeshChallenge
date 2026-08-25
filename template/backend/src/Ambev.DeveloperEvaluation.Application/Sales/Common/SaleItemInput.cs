namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Shared item-input shape used by both CreateSale and UpdateSale commands.
/// </summary>
public class SaleItemInput
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
