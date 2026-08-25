using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Handler for processing CancelSaleCommand requests
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly ISalesListCache _cache;

    public CancelSaleHandler(ISaleRepository saleRepository, IMapper mapper, IBus bus, ISalesListCache cache)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _bus = bus;
        _cache = cache;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.Cancel();

        var cancelledSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _cache.InvalidateAsync(cancellationToken);

        await _bus.Publish(new SaleCancelledEvent(cancelledSale.Id, cancelledSale.SaleNumber));

        return _mapper.Map<CancelSaleResult>(cancelledSale);
    }
}
