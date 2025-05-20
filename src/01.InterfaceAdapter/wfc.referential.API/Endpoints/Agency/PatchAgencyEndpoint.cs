using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Agencies.Commands.PatchAgency;
using wfc.referential.Application.Agencies.Dtos;


namespace wfc.referential.API.Endpoints.Agency;

public class PatchAgencyEndpoint(IMediator _mediator)
    : Endpoint<PatchAgencyRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/agencies/{AgencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update an Agency";
            s.Description =
                "Updates only the supplied fields for the agency identified by {AgencyId}.";
            s.Params["AgencyId"] = "Agency GUID from route";

            s.Response<Guid>(200, "Returns updated AgencyId");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Agency not found");
        });
        Options(o => o.WithTags(EndpointGroups.Agencies));
    }

    public override async Task HandleAsync(PatchAgencyRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchAgencyCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}