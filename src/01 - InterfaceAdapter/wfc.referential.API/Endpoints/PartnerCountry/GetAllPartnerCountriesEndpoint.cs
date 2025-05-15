using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerCountries.Dtos;
using wfc.referential.Application.PartnerCountries.Queries.GetAllPartnerCountries;

namespace wfc.referential.API.Endpoints.PartnerCountry;

public class GetAllPartnerCountriesEndpoint(IMediator _mediator)
    : Endpoint<GetAllPartnerCountriesRequest, PagedResult<PartnerCountryResponse>>
{
    public override void Configure()
    {
        Get("/api/partnerCountries");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get paginated Partner–Country mappings";
            s.Description = """
                Returns a paginated list of PartnerCountry rows.
                Supports optional filtering by PartnerId, CountryId and IsEnabled.
                """;

            s.Response<PagedResult<PartnerCountryResponse>>(200, "Successful response");
            s.Response(400, "Invalid paging / filter parameters");
            s.Response(500, "Unexpected server error");
        });

        Options(o => o.WithTags(EndpointGroups.PartnerCountries));
    }

    public override async Task HandleAsync(GetAllPartnerCountriesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllPartnerCountriesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}