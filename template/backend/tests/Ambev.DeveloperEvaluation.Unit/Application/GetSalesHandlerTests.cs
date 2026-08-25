using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Common.Pagination;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSalesHandler"/> class, focused
/// on the generation-based Sales list cache (see <see cref="ISalesListCache"/>).
/// </summary>
public class GetSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ISalesListCache _cache;
    private readonly GetSalesHandler _handler;

    public GetSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _cache = Substitute.For<ISalesListCache>();
        _handler = new GetSalesHandler(_saleRepository, _mapper, _cache);
    }

    /// <summary>
    /// Tests that a cache hit short-circuits the request — the repository is
    /// never queried.
    /// </summary>
    [Fact(DisplayName = "Given a cached result When listing sales Then returns it without querying the repository")]
    public async Task Handle_CacheHit_ReturnsCachedResultWithoutQueryingRepository()
    {
        // Given
        var command = new GetSalesCommand { PageNumber = 1, PageSize = 10 };
        var cached = new PagedResult<GetSaleResult>
        {
            Items = [new GetSaleResult { Id = Guid.NewGuid() }],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _cache.GetAsync(command, Arg.Any<CancellationToken>()).Returns(cached);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().BeSameAs(cached);
        await _saleRepository.DidNotReceive().GetPagedAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that a cache miss falls back to the repository and populates
    /// the cache with the fresh result.
    /// </summary>
    [Fact(DisplayName = "Given no cached result When listing sales Then queries the repository and caches the result")]
    public async Task Handle_CacheMiss_QueriesRepositoryAndCachesResult()
    {
        // Given
        var command = new GetSalesCommand { PageNumber = 1, PageSize = 10 };
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        var mappedResult = new GetSaleResult { Id = sale.Id };

        _cache.GetAsync(command, Arg.Any<CancellationToken>()).Returns((PagedResult<GetSaleResult>?)null);
        _saleRepository.GetPagedAsync(
            command.PageNumber, command.PageSize, command.IsCancelled, command.CustomerId, command.BranchId,
            command.StartDate, command.EndDate, command.OrderBy, command.Descending, Arg.Any<CancellationToken>())
            .Returns((new List<Sale> { sale }, 1));
        _mapper.Map<IEnumerable<GetSaleResult>>(Arg.Any<IEnumerable<Sale>>()).Returns([mappedResult]);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Items.Should().ContainSingle().Which.Should().Be(mappedResult);
        result.TotalCount.Should().Be(1);
        await _cache.Received(1).SetAsync(command, Arg.Is<PagedResult<GetSaleResult>>(r => r.TotalCount == 1), Arg.Any<CancellationToken>());
    }
}
