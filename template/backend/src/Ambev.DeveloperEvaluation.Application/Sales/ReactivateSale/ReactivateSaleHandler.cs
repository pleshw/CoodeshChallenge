using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.ReactivateSale;

/// <summary>
/// Handler for processing ReactivateSaleCommand requests
/// </summary>
public class ReactivateSaleHandler : IRequestHandler<ReactivateSaleCommand, ReactivateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IBus _bus;

    public ReactivateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IBus bus)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _bus = bus;
    }

    public async Task<ReactivateSaleResult> Handle(ReactivateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new ReactivateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.Reactivate();

        var reactivatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _bus.Publish(new SaleReactivatedEvent(reactivatedSale.Id, reactivatedSale.SaleNumber));

        return _mapper.Map<ReactivateSaleResult>(reactivatedSale);
    }
}
