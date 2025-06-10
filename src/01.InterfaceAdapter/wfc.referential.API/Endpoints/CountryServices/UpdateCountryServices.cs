using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryServices.Commands.UpdateCountryService;
using wfc.referential.Application.CountryServices.Dtos;

namespace wfc.referential.API.Endpoints.CountryServices;

public class UpdateCountryServices(IMediator _mediator)
    : Endpoint<UpdateCountryServiceRequest, bool>
{
    public override void Configure()
    {
        Put("/api/countryservices/{CountryServiceId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Country-Services association";
            s.Description = "Updates the association identified by {CountryServiceId} with supplied body fields.";
            s.Params["CountryServiceId"] = "Association GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Association not found");
            s.Response(409, "Conflict with an existing association");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.CountryServices));
    }

    public override async Task HandleAsync(UpdateCountryServiceRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateCountryServiceCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}