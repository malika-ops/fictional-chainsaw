using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;
using wfc.referential.Application.AgencyTiers.Dtos;

namespace wfc.referential.API.Endpoints.AgencyTier;

public class UpdateAgencyTier(IMediator _mediator) : Endpoint<UpdateAgencyTierRequest, bool>
{
    public override void Configure()
    {
        Put("/api/agencyTiers/{AgencyTierId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Update an existing Agency-Tier link";
            s.Description = "Replaces Code / Password / status (and optionally TierId, AgencyId) "
                          + "for the AgencyTier identified by the route param.";
            s.Params["AgencyTierId"] = "AgencyTier ID (GUID) from route";
            s.Response<bool>(200, "Returns a boolean (True or False) as the status of the updated AgencyTier");
            s.Response(400, "Validation errors");
            s.Response(404, "AgencyTier not found");
            s.Response(409, "Conflict with an existing AgencyTier");
        });

        Options(o => o.WithTags(EndpointGroups.AgencyTiers));
    }

    public override async Task HandleAsync(UpdateAgencyTierRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateAgencyTierCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}