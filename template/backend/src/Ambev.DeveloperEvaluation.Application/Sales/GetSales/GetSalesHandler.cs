using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Common.Pagination;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

/// <summary>
/// Handler for processing GetSalesCommand requests
/// </summary>
public class GetSalesHandler : IRequestHandler<GetSalesCommand, PagedResult<GetSaleResult>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ISalesListCache _cache;

    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper, ISalesListCache cache)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PagedResult<GetSaleResult>> Handle(GetSalesCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetSalesValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cached = await _cache.GetAsync(request, cancellationToken);
        if (cached is not null)
            return cached;

        var (sales, totalCount) = await _saleRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.IsCancelled,
            request.CustomerId,
            request.BranchId,
            request.StartDate,
            request.EndDate,
            request.OrderBy,
            request.Descending,
            cancellationToken);

        var result = new PagedResult<GetSaleResult>
        {
            Items = _mapper.Map<IEnumerable<GetSaleResult>>(sales),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        await _cache.SetAsync(request, result, cancellationToken);

        return result;
    }
}
