using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation to ensure consistency
/// across test cases and provide both valid and invalid data scenarios.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<SaleItemInput> SaleItemInputFaker = new Faker<SaleItemInput>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(1, 100))
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 3));

    private static readonly Faker<CreateSaleCommand> CreateSaleCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Company.CompanyName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Address.City())
        .RuleFor(s => s.Items, f => SaleItemInputFaker.Generate(2));

    /// <summary>
    /// Generates a valid CreateSaleCommand with randomized data that meets
    /// the system's validation requirements.
    /// </summary>
    public static CreateSaleCommand GenerateValidCommand()
    {
        return CreateSaleCommandFaker.Generate();
    }
}
