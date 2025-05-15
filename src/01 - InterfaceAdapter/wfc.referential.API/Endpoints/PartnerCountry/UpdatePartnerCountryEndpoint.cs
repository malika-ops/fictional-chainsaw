using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;
using wfc.referential.Application.PartnerCountries.Dtos;


namespace wfc.referential.API.Endpoints.PartnerCountry;

public class UpdatePartnerCountryEndpoint(IMediator _mediator)
    : Endpoint<UpdatePartnerCountryRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/partnerCountries/{PartnerCountryId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Update an existing Partner–Country link";
            s.Description = "Re-assigns the link or toggles IsEnabled.";
            s.Params["PartnerCountryId"] = "PartnerCountry GUID (route)";

            s.Response<Guid>(200, "Updated PartnerCountry Id");
            s.Response(400, "Validation failed");
            s.Response(404, "Entity not found");
        });

        Options(o => o.WithTags(EndpointGroups.PartnerCountries));
    }

    public override async Task HandleAsync(UpdatePartnerCountryRequest dto, CancellationToken ct)
    {
        var cmd = dto.Adapt<UpdatePartnerCountryCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
