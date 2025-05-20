using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class GetAllCountryIdentityDocs(IMediator _mediator) : Endpoint<GetAllCountryIdentityDocsRequest, PagedResult<GetCountryIdentityDocResponse>>
{
    public override void Configure()
    {
        Get("/api/countryidentitydocs");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Country-Identity Document associations";
            s.Description = "Returns a paginated list of associations. Supports optional filtering by CountryId, IdentityDocumentId, and IsEnabled.";

            s.Response<PagedResult<GetCountryIdentityDocResponse>>(200, "Paged list of associations");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(GetAllCountryIdentityDocsRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllCountryIdentityDocsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}