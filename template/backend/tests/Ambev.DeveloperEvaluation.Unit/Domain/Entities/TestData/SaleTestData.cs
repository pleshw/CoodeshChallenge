using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation to ensure consistency
/// across test cases and provide both valid and invalid data scenarios.
/// </summary>
public static class SaleTestData
{
    /// <summary>
    /// Configures the Faker to generate valid Sale header data (no items yet).
    /// </summary>
    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.SaleNumber, f => $"SALE-{f.Random.AlphaNumeric(10).ToUpperInvariant()}")
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Company.CompanyName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Address.City());

    /// <summary>
    /// Generates a valid Sale with a single valid item (quantity 3, no discount tier).
    /// </summary>
    /// <returns>A valid Sale entity ready to pass validation.</returns>
    public static Sale GenerateValidSale()
    {
        var sale = SaleFaker.Generate();
        sale.AddItem(Guid.NewGuid(), new Faker().Commerce.ProductName(), 10m, 3);
        return sale;
    }

    /// <summary>
    /// Generates a valid Sale header with no items, so tests can add items
    /// themselves to exercise specific quantities/discount tiers.
    /// </summary>
    /// <returns>A Sale entity with valid header data and an empty item list.</returns>
    public static Sale GenerateSaleWithoutItems()
    {
        return SaleFaker.Generate();
    }
}
