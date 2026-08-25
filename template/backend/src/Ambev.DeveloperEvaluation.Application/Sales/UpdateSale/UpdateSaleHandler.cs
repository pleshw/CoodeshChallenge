using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly ISalesListCache _cache;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IBus bus, ISalesListCache cache)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _bus = bus;
        _cache = cache;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;
        sale.ReplaceItems(command.Items.Select(i => (i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)));

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _cache.InvalidateAsync(cancellationToken);

        await _bus.Publish(new SaleModifiedEvent(updatedSale.Id, updatedSale.SaleNumber));

        return _mapper.Map<UpdateSaleResult>(updatedSale);
    }
}
