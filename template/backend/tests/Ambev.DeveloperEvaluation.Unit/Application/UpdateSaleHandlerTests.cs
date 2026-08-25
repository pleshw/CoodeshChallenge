using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _bus = Substitute.For<IBus>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _bus);

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
    }

    private static UpdateSaleCommand CreateValidCommand(Guid saleId) => new()
    {
        Id = saleId,
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Acme Corp",
        BranchId = Guid.NewGuid(),
        BranchName = "Downtown Branch",
        Items = [new SaleItemInput { ProductId = Guid.NewGuid(), ProductName = "Beer", UnitPrice = 10m, Quantity = 10 }]
    };

    /// <summary>
    /// Tests that updating a non-existent sale throws a not-found exception.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale When updating Then throws key not found exception")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var command = CreateValidCommand(Guid.NewGuid());
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that updating a sale fully replaces its item list and
    /// recalculates the total using the new items' discount tiers.
    /// </summary>
    [Fact(DisplayName = "Given existing sale When updating Then replaces items and recalculates total")]
    public async Task Handle_ValidRequest_ReplacesItemsAndRecalculatesTotal()
    {
        // Given
        var existingSale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        existingSale.AddItem(Guid.NewGuid(), "Old Product", 5m, 2);

        var command = CreateValidCommand(existingSale.Id);

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>()).Returns(new UpdateSaleResult { Id = existingSale.Id });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        existingSale.Items.Should().ContainSingle(i => i.ProductName == "Beer" && i.Discount == 0.20m);
        existingSale.TotalAmount.Should().Be(10m * 10 * 0.8m);
    }

    /// <summary>
    /// Tests that updating a sale publishes a SaleModifiedEvent.
    /// </summary>
    [Fact(DisplayName = "Given existing sale When updated Then publishes SaleModifiedEvent")]
    public async Task Handle_ValidRequest_PublishesSaleModifiedEvent()
    {
        // Given
        var existingSale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        existingSale.AddItem(Guid.NewGuid(), "Old Product", 5m, 2);

        var command = CreateValidCommand(existingSale.Id);

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>()).Returns(new UpdateSaleResult { Id = existingSale.Id });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _bus.Received(1).Publish(Arg.Any<SaleModifiedEvent>());
    }
}
