using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;
using wfc.referential.Application.AgencyTiers.Dtos;


namespace wfc.referential.API.Endpoints.AgencyTier;

public class DeleteAgencyTier(IMediator _mediator)
    : Endpoint<DeleteAgencyTierRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/agencyTiers/{AgencyTierId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete an Agency-Tier mapping by GUID";
            s.Description = "Soft-deletes the AgencyTier identified by {AgencyTierId} "
                          + "by disabling it (IsEnabled = false).";
            s.Params["AgencyTierId"] = "Agency-Tier ID (GUID) from route";

            s.Response<bool>(200, "True if deletion (disable) succeeded");
            s.Response(400, "Returned if validation failed");
        });

        Options(o => o.WithTags(EndpointGroups.AgencyTiers));
    }

    public override async Task HandleAsync(DeleteAgencyTierRequest dto, CancellationToken ct)
    {
        var cmd = dto.Adapt<DeleteAgencyTierCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}