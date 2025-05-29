using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.PatchPartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class PatchPartnerEndpoint(IMediator _mediator) : Endpoint<PatchPartnerRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/partners/{PartnerId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Partner";
            s.Description = "Updates only the supplied fields for the partner identified by {PartnerId}.";
            s.Params["PartnerId"] = "Partner GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Partner not found");
            s.Response(409, "Conflict with an existing Partner");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(PatchPartnerRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchPartnerCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}