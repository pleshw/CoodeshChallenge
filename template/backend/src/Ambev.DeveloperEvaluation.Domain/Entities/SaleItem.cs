using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a single product line within a Sale, denormalized from the
/// Product bounded context following the External Identities pattern.
/// </summary>
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    /// <summary>
    /// The discount percentage applied to this item (e.g. 0.10 = 10%), derived
    /// from the quantity-based discount tiers.
    /// </summary>
    public decimal Discount { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsCancelled { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Cancels this item. Only called through <see cref="Sale.CancelItem"/> so the
    /// owning sale's total is always recalculated alongside it.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
