using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.ReactivateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="ReactivateSaleHandler"/> class.
/// </summary>
public class ReactivateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly ReactivateSaleHandler _handler;

    public ReactivateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _bus = Substitute.For<IBus>();
        _handler = new ReactivateSaleHandler(_saleRepository, _mapper, _bus);

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
    }

    /// <summary>
    /// Tests that reactivating a non-existent sale throws a not-found exception.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale When reactivating Then throws key not found exception")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Given
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new ReactivateSaleCommand(saleId), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that reactivating a sale that isn't cancelled throws a domain exception.
    /// </summary>
    [Fact(DisplayName = "Given uncancelled sale When reactivating Then throws domain exception")]
    public async Task Handle_SaleNotCancelled_ThrowsDomainException()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(new ReactivateSaleCommand(sale.Id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
    }

    /// <summary>
    /// Tests that reactivating a cancelled sale clears IsCancelled and
    /// publishes a SaleReactivatedEvent.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When reactivated Then clears IsCancelled and publishes event")]
    public async Task Handle_ValidRequest_ReactivatesSaleAndPublishesEvent()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        sale.Cancel();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<ReactivateSaleResult>(Arg.Any<Sale>()).Returns(new ReactivateSaleResult { Id = sale.Id });

        // When
        await _handler.Handle(new ReactivateSaleCommand(sale.Id), CancellationToken.None);

        // Then
        sale.IsCancelled.Should().BeFalse();
        sale.CancelledAt.Should().BeNull();
        await _bus.Received(1).Publish(Arg.Any<SaleReactivatedEvent>());
    }
}
