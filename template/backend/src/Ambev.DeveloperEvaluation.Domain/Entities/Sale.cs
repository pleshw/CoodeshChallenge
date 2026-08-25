using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale record. References to Customer, Branch and Product are
/// denormalized (id + name) following the DDD External Identities pattern,
/// since those bounded contexts don't exist in this codebase.
/// </summary>
public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Sum of all non-cancelled items' <see cref="SaleItem.TotalAmount"/>.
    /// Recalculated automatically whenever items are added or cancelled.
    /// </summary>
    public decimal TotalAmount { get; set; }

    public bool IsCancelled { get; set; }

    /// <summary>
    /// When the sale was cancelled. Tracked separately from <see cref="UpdatedAt"/>
    /// so cancellation timing survives later, unrelated updates, and so
    /// <see cref="Reactivate"/> has a clear signal to clear.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<SaleItem> Items { get; private set; } = new List<SaleItem>();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a product line to the sale, applying the quantity-based discount
    /// tiers and recalculating the sale total. This is the only way items should
    /// be added, so the discount rule can never be bypassed by constructing a
    /// <see cref="SaleItem"/> directly.
    /// </summary>
    public SaleItem AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var discount = CalculateDiscountPercentage(quantity);
        var item = new SaleItem
        {
            SaleId = Id,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Discount = discount,
            TotalAmount = unitPrice * quantity * (1 - discount)
        };

        Items.Add(item);
        RecalculateTotal();

        return item;
    }

    /// <summary>
    /// Replaces the entire item list (used by Update, which treats the item
    /// list as a full replace rather than granular add/remove operations).
    /// </summary>
    public void ReplaceItems(IEnumerable<(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity)> items)
    {
        Items.Clear();

        foreach (var item in items)
            AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reverts a whole-sale cancellation. Does not touch individual cancelled
    /// items — those remain cancelled and must be reactivated separately, if
    /// that's ever needed.
    /// </summary>
    public void Reactivate()
    {
        if (!IsCancelled)
            throw new DomainException("Sale is not cancelled.");

        IsCancelled = false;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels a single item. Independent from <see cref="Cancel"/> — cancelling
    /// the whole sale does not cancel individual items and vice versa.
    /// </summary>
    public void CancelItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Sale item with ID {itemId} not found");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    /// <summary>
    /// Enforces the discount tiers: below 4 items no discount, 4-9 items 10%,
    /// 10-20 items 20%, more than 20 identical items is not allowed.
    /// </summary>
    private static decimal CalculateDiscountPercentage(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Quantity must be at least 1.");
        if (quantity > 20)
            throw new DomainException("Cannot sell more than 20 identical items.");
        if (quantity >= 10)
            return 0.20m;
        if (quantity >= 4)
            return 0.10m;

        return 0m;
    }
}
