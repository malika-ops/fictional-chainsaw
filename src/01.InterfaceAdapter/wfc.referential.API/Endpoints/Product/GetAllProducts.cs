using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Application.Products.Queries.GetAllProducts;

namespace wfc.referential.API.Endpoints.Product;

public class GetAllProducts(IMediator _mediator) : Endpoint<GetAllProductsRequest, PagedResult<GetAllProductsResponse>>
{
    public override void Configure()
    {
        Get("api/products");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Products";
            s.Response<PagedResult<GetAllProductsResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Products));
    }

    public override async Task HandleAsync(GetAllProductsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllProductsQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}