using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Countries.Commands.PatchCountry;
using wfc.referential.Application.Countries.Dtos;


namespace wfc.referential.API.Endpoints.Country;

public class PatchCountryEndpoint(IMediator _mediator) : Endpoint<PatchCountryRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/countries/{CountryId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Country";
            s.Description = "Updates only the supplied fields of the specified Country.";
            s.Params["CountryId"] = "Country ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated Country");
            s.Response(400, "Validation fails");
            s.Response(404, "Country not found");
        });
        Options(o => o.WithTags(EndpointGroups.Countries));
    }

    public override async Task HandleAsync(PatchCountryRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchCountryCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}