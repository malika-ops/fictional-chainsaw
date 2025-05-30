using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.IdentityDocuments.Queries.GetAllIdentityDocuments;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class GetAllIdentityDocumentsEndpoint(IMediator _mediator)
    : Endpoint<GetAllIdentityDocumentsRequest, PagedResult<GetIdentityDocumentsResponse>>
{
    public override void Configure()
    {
        Get("/api/identitydocuments");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of identity documents";
            s.Description = """
                Returns a paginated list of identity documents.
                Filters supported: code, name, status.
                """;
            s.Response<PagedResult<GetIdentityDocumentsResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
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