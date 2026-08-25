using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

public class CancelSaleProfile : Profile
{
    public CancelSaleProfile()
    {
        CreateMap<Guid, Application.Sales.CancelSale.CancelSaleCommand>()
            .ConstructUsing(id => new Application.Sales.CancelSale.CancelSaleCommand(id));

        CreateMap<Application.Sales.CancelSale.CancelSaleResult, CancelSaleResponse>();
    }
}
