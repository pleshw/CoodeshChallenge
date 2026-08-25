using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the SaleItemValidator class.
/// </summary>
public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator;

    public SaleItemValidatorTests()
    {
        _validator = new SaleItemValidator();
    }

    private static SaleItem CreateValidItem() => new()
    {
        ProductId = Guid.NewGuid(),
        ProductName = "Beer Can 350ml",
        Quantity = 5,
        UnitPrice = 10m,
        Discount = 0.10m
    };

    [Fact(DisplayName = "Valid sale item should pass all validation rules")]
    public void Given_ValidSaleItem_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var item = CreateValidItem();

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty product ID should fail validation")]
    public void Given_EmptyProductId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateValidItem();
        item.ProductId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact(DisplayName = "Empty product name should fail validation")]
    public void Given_EmptyProductName_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateValidItem();
        item.ProductName = string.Empty;

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Theory(DisplayName = "Quantity outside 1-20 should fail validation")]
    [InlineData(0)]
    [InlineData(21)]
    public void Given_QuantityOutsideAllowedRange_When_Validated_Then_ShouldHaveError(int quantity)
    {
        // Arrange
        var item = CreateValidItem();
        item.Quantity = quantity;

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact(DisplayName = "Unit price of zero should fail validation")]
    public void Given_ZeroUnitPrice_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var item = CreateValidItem();
        item.UnitPrice = 0m;

        // Act
        var result = _validator.TestValidate(item);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }
}
