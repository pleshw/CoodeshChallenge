using Ambev.DeveloperEvaluation.Application.Sales.Common;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

/// <summary>
/// Shared item-level mappings, reused by every operation's own Request -> Command
/// and Result -> Response profile via AutoMapper's collection-mapping convention.
/// </summary>
public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<SaleItemRequest, SaleItemInput>();
        CreateMap<SaleItemResult, SaleItemResponse>();
    }
}
