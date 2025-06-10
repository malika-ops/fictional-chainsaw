using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryServices.Commands.PatchCountryService;
using wfc.referential.Application.CountryServices.Dtos;

namespace wfc.referential.API.Endpoints.CountryServices;

public class PatchCountryServices(IMediator _mediator)
    : Endpoint<PatchCountryServiceRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/countryservices/{CountryServiceId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Country-Services association";
            s.Description =
                "Updates only the supplied fields for the association identified by {CountryServiceId}.";
            s.Params["CountryServiceId"] = "Association GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Association not found");
            s.Response(409, "Conflict with an existing association");
        });
        Options(o => o.WithTags(EndpointGroups.CountryServices));
    }

    public override async Task HandleAsync(PatchCountryServiceRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchCountryServiceCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}