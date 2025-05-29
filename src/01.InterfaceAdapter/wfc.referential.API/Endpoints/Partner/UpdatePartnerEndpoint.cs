using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.UpdatePartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class UpdatePartnerEndpoint(IMediator _mediator) : Endpoint<UpdatePartnerRequest, bool>
{
    public override void Configure()
    {
        Put("/api/partners/{PartnerId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Partner";
            s.Description = "Updates the partner identified by {PartnerId} with supplied body fields.";
            s.Params["PartnerId"] = "Partner GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Partner not found");
            s.Response(409, "Conflict with an existing Partner");
            s.Response(500, "Unexpected server error");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(UpdatePartnerRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdatePartnerCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}