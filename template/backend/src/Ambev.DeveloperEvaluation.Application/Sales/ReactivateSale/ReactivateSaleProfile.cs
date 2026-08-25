using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.ReactivateSale;

public class ReactivateSaleProfile : Profile
{
    public ReactivateSaleProfile()
    {
        CreateMap<Sale, ReactivateSaleResult>();
    }
}
