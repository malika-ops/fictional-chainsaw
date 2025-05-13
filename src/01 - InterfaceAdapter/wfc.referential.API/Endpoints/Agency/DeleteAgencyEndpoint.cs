using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Agencies.Commands.DeleteAgency;
using wfc.referential.Application.Agencies.Dtos;

namespace wfc.referential.API.Endpoints.Agency;

public class DeleteAgencyEndpoint(IMediator _mediator)
    : Endpoint<DeleteAgencyRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/agencies/{AgencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete an agency by GUID";
            s.Description = "Soft-deletes the agency identified by {AgencyId}.";
            s.Params["AgencyId"] = "GUID of the agency to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
        });
        Options(o => o.WithTags(EndpointGroups.Agencies));
    }

    public override async Task HandleAsync(DeleteAgencyRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteAgencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}