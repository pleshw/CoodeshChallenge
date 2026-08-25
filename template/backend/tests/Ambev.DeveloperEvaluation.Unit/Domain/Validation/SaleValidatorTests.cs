using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

/// <summary>
/// Contains unit tests for the SaleValidator class.
/// </summary>
public class SaleValidatorTests
{
    private readonly SaleValidator _validator;

    public SaleValidatorTests()
    {
        _validator = new SaleValidator();
    }

    [Fact(DisplayName = "Valid sale should pass all validation rules")]
    public void Given_ValidSale_When_Validated_Then_ShouldNotHaveErrors()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Sale without items should fail validation")]
    public void Given_SaleWithoutItems_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateSaleWithoutItems();

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact(DisplayName = "Empty customer ID should fail validation")]
    public void Given_EmptyCustomerId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.CustomerId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact(DisplayName = "Empty branch ID should fail validation")]
    public void Given_EmptyBranchId_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.BranchId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BranchId);
    }

    [Fact(DisplayName = "Default sale date should fail validation")]
    public void Given_DefaultSaleDate_When_Validated_Then_ShouldHaveError()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.SaleDate = default;

        // Act
        var result = _validator.TestValidate(sale);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SaleDate);
    }
}
