using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetAllCountryIdentityDocs;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class GetAllCountryIdentityDocsEndpoint(IMediator _mediator)
    : Endpoint<GetAllCountryIdentityDocsRequest, PagedResult<GetCountryIdentityDocsResponse>>
{
    public override void Configure()
    {
        Get("/api/countryidentitydocs");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of country-identity document associations";
            s.Description = """
                Returns a paginated list of country-identity document associations.
                Filters supported: countryId, identityDocumentId, status.
                """;
            s.Response<PagedResult<GetCountryIdentityDocsResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(GetAllCountryIdentityDocsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllCountryIdentityDocsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}