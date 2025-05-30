using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Agencies.Commands.UpdateAgency;
using wfc.referential.Application.Agencies.Dtos;


namespace wfc.referential.API.Endpoints.Agency;

public class UpdateAgencyEndpoint(IMediator _mediator)
    : Endpoint<UpdateAgencyRequest, bool>
{
    public override void Configure()
    {
        Put("/api/agencies/{AgencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Agency";
            s.Description = "Updates the agency identified by {AgencyId} with supplied body fields.";
            s.Params["AgencyId"] = "Agency GUID (from route)";

            s.Response<bool>(200, "true on success");
            s.Response(400, "Validation or business rule failure");
            s.Response(500, "Unexpected server error");
            s.Response(409, "Conflict with an existing Agency");
        });
        Options(o => o.WithTags(EndpointGroups.Agencies));
    }

    public override async Task HandleAsync(UpdateAgencyRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateAgencyCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}