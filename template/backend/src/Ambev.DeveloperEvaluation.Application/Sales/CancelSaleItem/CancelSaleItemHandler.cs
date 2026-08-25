using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Handler for processing CancelSaleItemCommand requests
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IMediator mediator)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        var item = sale.Items.FirstOrDefault(i => i.Id == request.ItemId)
            ?? throw new KeyNotFoundException($"Sale item with ID {request.ItemId} not found");

        sale.CancelItem(request.ItemId);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _mediator.Publish(
            new ItemCancelledEvent(updatedSale.Id, item.Id, item.ProductId, item.ProductName),
            cancellationToken);

        return new CancelSaleItemResult
        {
            SaleId = updatedSale.Id,
            ItemId = item.Id,
            IsCancelled = item.IsCancelled,
            SaleTotalAmount = updatedSale.TotalAmount
        };
    }
}
