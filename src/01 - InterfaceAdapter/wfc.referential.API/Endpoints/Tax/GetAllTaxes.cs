using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Application.Taxes.Queries.GetAllTaxes;

namespace wfc.referential.API.Endpoints.Tax;

public class GetAllTaxes(IMediator _mediator) : Endpoint<GetAllTaxesRequest, PagedResult<GetAllTaxesResponse>>
{
    public override void Configure()
    {
        Get("api/taxes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of taxes";
            s.Response<PagedResult<GetAllTaxesResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Taxes));
    }

    public override async Task HandleAsync(GetAllTaxesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllTaxesQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}