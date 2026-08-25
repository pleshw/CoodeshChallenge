using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ReactivateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Controller for managing sale operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateSaleCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);
        var data = _mapper.Map<CreateSaleResponse>(response);

        // StatusCode(...) is used instead of the BaseController.Created<T>/Ok<T> helpers:
        // those helpers wrap whatever is passed into another ApiResponseWithData<T>, and
        // since this action already builds the wrapper by hand (to set a custom Message),
        // going through them here would double-wrap the response body.
        return StatusCode(StatusCodes.Status201Created, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = data
        });
    }

    /// <summary>
    /// Retrieves a sale by its ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetSaleRequest { Id = id };
        var validator = new GetSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<GetSaleCommand>(request.Id);
        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, new ApiResponseWithData<GetSaleResponse>
        {
            Success = true,
            Message = "Sale retrieved successfully",
            Data = _mapper.Map<GetSaleResponse>(response)
        });
    }

    /// <summary>
    /// Retrieves a paginated, filtered, ordered list of sales
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSales(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery] bool? cancelled = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery(Name = "_order")] string? order = null,
        CancellationToken cancellationToken = default)
    {
        var (orderBy, descending) = ParseOrder(order);

        var command = new GetSalesCommand
        {
            PageNumber = page,
            PageSize = size,
            IsCancelled = cancelled,
            CustomerId = customerId,
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate,
            OrderBy = orderBy,
            Descending = descending
        };

        var result = await _mediator.Send(command, cancellationToken);

        var items = _mapper.Map<List<GetSaleResponse>>(result.Items);
        var pagedList = new PaginatedList<GetSaleResponse>(items, result.TotalCount, result.PageNumber, result.PageSize);

        return StatusCode(StatusCodes.Status200OK, new PaginatedResponse<GetSaleResponse>
        {
            Success = true,
            Data = pagedList,
            CurrentPage = pagedList.CurrentPage,
            TotalPages = pagedList.TotalPages,
            TotalCount = pagedList.TotalCount
        });
    }

    /// <summary>
    /// Updates an existing sale
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, new ApiResponseWithData<UpdateSaleResponse>
        {
            Success = true,
            Message = "Sale updated successfully",
            Data = _mapper.Map<UpdateSaleResponse>(response)
        });
    }

    /// <summary>
    /// Deletes a sale by its ID
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteSaleRequest { Id = id };
        var validator = new DeleteSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<DeleteSaleCommand>(request.Id);
        await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, new ApiResponse
        {
            Success = true,
            Message = "Sale deleted successfully"
        });
    }

    /// <summary>
    /// Cancels a whole sale
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new CancelSaleRequest { Id = id };
        var validator = new CancelSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CancelSaleCommand>(request.Id);
        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, new ApiResponseWithData<CancelSaleResponse>
        {
            Success = true,
            Message = "Sale cancelled successfully",
            Data = _mapper.Map<CancelSaleResponse>(response)
        });
    }

    /// <summary>
    /// Reverts a whole-sale cancellation
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(typeof(ApiResponseWithData<ReactivateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new ReactivateSaleRequest { Id = id };
        var validator = new ReactivateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<Application.Sales.ReactivateSale.ReactivateSaleCommand>(request.Id);
        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, new ApiResponseWithData<ReactivateSaleResponse>
        {
            Success = true,
            Message = "Sale reactivated successfully",
            Data = _mapper.Map<ReactivateSaleResponse>(response)
        });
    }

    /// <summary>
    /// Cancels a single item within a sale
    /// </summary>
    [HttpPost("{id}/items/{itemId}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSaleItem([FromRoute] Guid id, [FromRoute] Guid itemId, CancellationToken cancellationToken)
    {
        var request = new CancelSaleItemRequest { SaleId = id, ItemId = itemId };
        var validator = new CancelSaleItemRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = new CancelSaleItemCommand(request.SaleId, request.ItemId);
        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status200OK, new ApiResponseWithData<CancelSaleItemResponse>
        {
            Success = true,
            Message = "Sale item cancelled successfully",
            Data = _mapper.Map<CancelSaleItemResponse>(response)
        });
    }

    private static (string? orderBy, bool descending) ParseOrder(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return (null, false);

        var parts = order.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0];
        var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (field, descending);
    }
}
