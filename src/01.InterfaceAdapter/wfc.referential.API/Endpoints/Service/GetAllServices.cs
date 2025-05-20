using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Application.Services.Queries.GetAllServices;

namespace wfc.referential.API.Endpoints.Service;

public class GetAllServices(IMediator _mediator) : Endpoint<GetAllServicesRequest, PagedResult<GetAllServicesResponse>>
{
    public override void Configure()
    {
        Get("api/services");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Services";
            s.Response<PagedResult<GetAllServicesResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Services));
    }

    public override async Task HandleAsync(GetAllServicesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllServicesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result,200, cancellation: ct);
    }
}