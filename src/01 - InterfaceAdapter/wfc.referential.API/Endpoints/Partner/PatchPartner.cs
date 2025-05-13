using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.PatchPartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class PatchPartner(IMediator _mediator) : Endpoint<PatchPartnerRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/partners/{PartnerId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Partner's properties";
            s.Description = "Updates only the provided fields of the specified partner ID.";
            s.Params["PartnerId"] = "Partner ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated partner");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Partner not found");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(PatchPartnerRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchPartnerCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}