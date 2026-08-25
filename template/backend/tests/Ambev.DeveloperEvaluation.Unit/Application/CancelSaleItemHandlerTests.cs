using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CancelSaleItemHandler"/> class.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IBus _bus;
    private readonly ISalesListCache _cache;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _bus = Substitute.For<IBus>();
        _cache = Substitute.For<ISalesListCache>();
        _handler = new CancelSaleItemHandler(_saleRepository, _bus, _cache);

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
    }

    /// <summary>
    /// Tests that cancelling an item on a non-existent sale throws.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale When cancelling item Then throws key not found exception")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new CancelSaleItemCommand(saleId, itemId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that cancelling a non-existent item within an existing sale throws.
    /// </summary>
    [Fact(DisplayName = "Given non-existent item When cancelling Then throws key not found exception")]
    public async Task Handle_ItemNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        sale.AddItem(Guid.NewGuid(), "Product", 10m, 3);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(new CancelSaleItemCommand(sale.Id, Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that cancelling an item excludes it from the sale total and
    /// publishes an ItemCancelledEvent, without cancelling the whole sale.
    /// </summary>
    [Fact(DisplayName = "Given valid item When cancelled Then excludes it from sale total and publishes event")]
    public async Task Handle_ValidRequest_CancelsItemAndPublishesEvent()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        var item1 = sale.AddItem(Guid.NewGuid(), "Product A", 10m, 3);
        var item2 = sale.AddItem(Guid.NewGuid(), "Product B", 10m, 4);
        // Items only receive a real Id from the database on save (gen_random_uuid());
        // simulate that here since these tests never touch a real DbContext.
        item1.Id = Guid.NewGuid();
        item2.Id = Guid.NewGuid();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(new CancelSaleItemCommand(sale.Id, item1.Id), CancellationToken.None);

        // Then
        result.IsCancelled.Should().BeTrue();
        result.SaleTotalAmount.Should().Be(item2.TotalAmount);
        sale.IsCancelled.Should().BeFalse();
        await _bus.Received(1).Publish(Arg.Any<ItemCancelledEvent>());
        await _cache.Received(1).InvalidateAsync(Arg.Any<CancellationToken>());
    }
}
