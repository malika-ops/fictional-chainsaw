using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;
using wfc.referential.Application.PartnerCountries.Dtos;

namespace wfc.referential.API.Endpoints.PartnerCountry;

public class CreatePartnerCountryEndpoint(IMediator _mediator)
    : Endpoint<CreatePartnerCountryRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/partner-countries");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Create a Partner-Country link";
            s.Description = """
                Creates a new row in `PartnerCountries` that links a Partner
                to a Country and returns the generated identifier.
                The (PartnerId, CountryId) pair must be unique.
                """;

            s.Response<Guid>(200, "Identifier of the new PartnerCountry row");
            s.Response(400, "Validation / business-rule failure");
            s.Response(500, "Unexpected server error");
        });

        Options(o => o.WithTags(EndpointGroups.PartnerCountries));
    }

    public override async Task HandleAsync(CreatePartnerCountryRequest req,
                                           CancellationToken ct)
    {
        var cmd = req.Adapt<CreatePartnerCountryCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}