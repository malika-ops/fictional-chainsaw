using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class GetAllIdentityDocuments(IMediator _mediator) : Endpoint<GetAllIdentityDocumentsRequest, PagedResult<GetAllIdentityDocumentsResponse>>
{
    public override void Configure()
    {
        Get("api/identitydocuments");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of IdentityDocuments";
            s.Response<PagedResult<GetAllIdentityDocumentsResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(GetAllIdentityDocumentsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllIdentityDocumentsQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}