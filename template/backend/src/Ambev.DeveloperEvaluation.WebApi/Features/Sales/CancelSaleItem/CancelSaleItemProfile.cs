using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemProfile : Profile
{
    public CancelSaleItemProfile()
    {
        CreateMap<Application.Sales.CancelSaleItem.CancelSaleItemResult, CancelSaleItemResponse>();
    }
}
