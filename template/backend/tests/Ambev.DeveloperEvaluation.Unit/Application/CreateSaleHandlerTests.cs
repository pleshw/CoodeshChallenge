using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _mediator = Substitute.For<IMediator>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _mediator);

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
    }

    /// <summary>
    /// Tests that a valid sale creation request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var result = new CreateSaleResult { Id = Guid.NewGuid() };

        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(result);

        // When
        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(result.Id);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that an invalid sale creation request throws a validation exception.
    /// </summary>
    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand(); // Empty command will fail validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that the >20 identical items business rule is rejected. In the
    /// full handler flow the command validator catches it first (defense in
    /// depth); the domain aggregate's own guard against the same rule is
    /// exercised directly, without going through command validation, in
    /// <c>SaleTests.Given_QuantityAboveTwenty_When_ItemAdded_Then_ThrowsDomainException</c>.
    /// </summary>
    [Fact(DisplayName = "Given item quantity above twenty When creating sale Then throws validation exception")]
    public async Task Handle_QuantityAboveTwenty_ThrowsValidationException()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items = [new SaleItemInput { ProductId = Guid.NewGuid(), ProductName = "Product", UnitPrice = 10m, Quantity = 21 }];

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that creating a sale publishes a SaleCreatedEvent for the
    /// application-log-based event mechanism.
    /// </summary>
    [Fact(DisplayName = "Given valid sale When created Then publishes SaleCreatedEvent")]
    public async Task Handle_ValidRequest_PublishesSaleCreatedEvent()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult { Id = Guid.NewGuid() });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _mediator.Received(1).Publish(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the discount tiers are applied correctly to the persisted
    /// sale before it is saved.
    /// </summary>
    [Fact(DisplayName = "Given item with 4 units When creating sale Then applies 10 percent discount")]
    public async Task Handle_ItemWithFourUnits_AppliesTenPercentDiscount()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items = [new SaleItemInput { ProductId = Guid.NewGuid(), ProductName = "Product", UnitPrice = 10m, Quantity = 4 }];
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult { Id = Guid.NewGuid() });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.Items.First().Discount == 0.10m && s.TotalAmount == 36m),
            Arg.Any<CancellationToken>());
    }
}
