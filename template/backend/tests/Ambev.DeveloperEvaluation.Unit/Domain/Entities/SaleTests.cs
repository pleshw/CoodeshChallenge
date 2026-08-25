using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale entity class.
/// Covers the quantity-based discount tiers, cancellation behavior, total
/// recalculation, and validation scenarios.
/// </summary>
public class SaleTests
{
    [Theory(DisplayName = "Items below 4 units should have no discount")]
    [InlineData(1)]
    [InlineData(3)]
    public void Given_QuantityBelowFour_When_ItemAdded_Then_NoDiscountIsApplied(int quantity)
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var item = sale.AddItem(Guid.NewGuid(), "Product", 10m, quantity);

        // Assert
        Assert.Equal(0m, item.Discount);
        Assert.Equal(10m * quantity, item.TotalAmount);
    }

    [Theory(DisplayName = "Items between 4 and 9 units should have a 10% discount")]
    [InlineData(4)]
    [InlineData(9)]
    public void Given_QuantityBetweenFourAndNine_When_ItemAdded_Then_TenPercentDiscountIsApplied(int quantity)
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var item = sale.AddItem(Guid.NewGuid(), "Product", 10m, quantity);

        // Assert
        Assert.Equal(0.10m, item.Discount);
        Assert.Equal(10m * quantity * 0.9m, item.TotalAmount);
    }

    [Theory(DisplayName = "Items between 10 and 20 units should have a 20% discount")]
    [InlineData(10)]
    [InlineData(20)]
    public void Given_QuantityBetweenTenAndTwenty_When_ItemAdded_Then_TwentyPercentDiscountIsApplied(int quantity)
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var item = sale.AddItem(Guid.NewGuid(), "Product", 10m, quantity);

        // Assert
        Assert.Equal(0.20m, item.Discount);
        Assert.Equal(10m * quantity * 0.8m, item.TotalAmount);
    }

    [Fact(DisplayName = "More than 20 identical items should not be allowed")]
    public void Given_QuantityAboveTwenty_When_ItemAdded_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var act = () => sale.AddItem(Guid.NewGuid(), "Product", 10m, 21);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact(DisplayName = "Zero or negative quantity should not be allowed")]
    public void Given_ZeroQuantity_When_ItemAdded_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var act = () => sale.AddItem(Guid.NewGuid(), "Product", 10m, 0);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact(DisplayName = "Sale total should be the sum of all non-cancelled item totals")]
    public void Given_MultipleItems_When_Added_Then_TotalAmountIsRecalculated()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", 10m, 3); // 30, no discount
        sale.AddItem(Guid.NewGuid(), "Product B", 10m, 4); // 36, 10% discount

        // Assert
        Assert.Equal(66m, sale.TotalAmount);
    }

    [Fact(DisplayName = "Cancelling the whole sale should not cancel individual items")]
    public void Given_Sale_When_Cancelled_Then_ItemsRemainUncancelled()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();
        var item = sale.AddItem(Guid.NewGuid(), "Product", 10m, 3);

        // Act
        sale.Cancel();

        // Assert
        Assert.True(sale.IsCancelled);
        Assert.False(item.IsCancelled);
        Assert.NotNull(sale.CancelledAt);
    }

    [Fact(DisplayName = "Reactivating a cancelled sale should clear IsCancelled and CancelledAt")]
    public void Given_CancelledSale_When_Reactivated_Then_IsCancelledAndCancelledAtAreCleared()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();
        sale.Cancel();

        // Act
        sale.Reactivate();

        // Assert
        Assert.False(sale.IsCancelled);
        Assert.Null(sale.CancelledAt);
    }

    [Fact(DisplayName = "Reactivating a sale that is not cancelled should throw")]
    public void Given_UncancelledSale_When_Reactivated_Then_ThrowsDomainException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var act = () => sale.Reactivate();

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact(DisplayName = "Cancelling an item should exclude it from the sale total but not cancel the sale")]
    public void Given_SaleItem_When_Cancelled_Then_TotalAmountExcludesIt()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();
        var item1 = sale.AddItem(Guid.NewGuid(), "Product A", 10m, 3); // 30
        var item2 = sale.AddItem(Guid.NewGuid(), "Product B", 10m, 4); // 36

        // Act
        sale.CancelItem(item1.Id);

        // Assert
        Assert.True(item1.IsCancelled);
        Assert.False(sale.IsCancelled);
        Assert.Equal(item2.TotalAmount, sale.TotalAmount);
    }

    [Fact(DisplayName = "Cancelling a non-existent item should throw")]
    public void Given_UnknownItemId_When_Cancelled_Then_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();
        sale.AddItem(Guid.NewGuid(), "Product", 10m, 3);

        // Act
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Assert
        Assert.Throws<KeyNotFoundException>(act);
    }

    [Fact(DisplayName = "Valid sale should pass validation")]
    public void Given_ValidSale_When_Validated_Then_ShouldReturnValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Sale without items should fail validation")]
    public void Given_SaleWithoutItems_When_Validated_Then_ShouldReturnInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
