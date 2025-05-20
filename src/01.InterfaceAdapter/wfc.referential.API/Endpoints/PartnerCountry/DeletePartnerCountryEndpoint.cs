using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;
using wfc.referential.Application.PartnerCountries.Dtos;

namespace wfc.referential.API.Endpoints.PartnerCountry;

public class DeletePartnerCountryEndpoint(IMediator _mediator)
    : Endpoint<DeletePartnerCountryRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/partnerCountries/{PartnerCountryId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Soft-delete a Partner–Country mapping";
            s.Description = "Sets IsEnabled = false on the specified PartnerCountry.";
            s.Params["PartnerCountryId"] = "PartnerCountry GUID (route)";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation failed or business rule violated");
        });

        Options(o => o.WithTags(EndpointGroups.PartnerCountries));
    }

    public override async Task HandleAsync(DeletePartnerCountryRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeletePartnerCountryCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}