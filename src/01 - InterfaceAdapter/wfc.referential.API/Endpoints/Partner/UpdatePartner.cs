using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.UpdatePartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class UpdatePartner(IMediator _mediator) : Endpoint<UpdatePartnerRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/partners/{PartnerId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Partner";
            s.Description = "Updates the partner identified by PartnerId with new details.";
            s.Params["PartnerId"] = "Partner ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated partner upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the partner does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(UpdatePartnerRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdatePartnerCommand>();
        var partnerId = await _mediator.Send(command, ct);
        await SendAsync(partnerId, cancellation: ct);
    }
}