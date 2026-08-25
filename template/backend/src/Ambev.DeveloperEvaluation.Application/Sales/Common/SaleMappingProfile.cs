using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Shared SaleItem -> SaleItemResult mapping, reused by every operation's own
/// Sale -> XResult profile via AutoMapper's collection-mapping convention.
/// </summary>
public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<SaleItem, SaleItemResult>();
    }
}
