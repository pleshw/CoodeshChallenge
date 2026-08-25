using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ReactivateSale;

public class ReactivateSaleProfile : Profile
{
    public ReactivateSaleProfile()
    {
        CreateMap<Guid, Application.Sales.ReactivateSale.ReactivateSaleCommand>()
            .ConstructUsing(id => new Application.Sales.ReactivateSale.ReactivateSaleCommand(id));

        CreateMap<Application.Sales.ReactivateSale.ReactivateSaleResult, ReactivateSaleResponse>();
    }
}
