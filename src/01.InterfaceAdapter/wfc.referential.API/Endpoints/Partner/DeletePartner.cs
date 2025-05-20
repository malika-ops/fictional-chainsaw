using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.DeletePartner;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.API.Endpoints.Partner;

public class DeletePartner(IMediator _mediator) : Endpoint<DeletePartnerRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/partners/{PartnerId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Partner by GUID";
            s.Description = "Deletes the Partner identified by {PartnerId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If partner was not found");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(DeletePartnerRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeletePartnerCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}