using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Taxes.Commands.UpdateTax;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.API.Endpoints.Tax;

public class UpdateTax(IMediator _mediator) : Endpoint<UpdateTaxRequest, Guid>
{
    public override void Configure()
    {
        Put("api/taxes/{TaxId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a Tax's properties";
            s.Description = "Updates all fields of the specified Tax ID.";
            s.Params["TaxId"] = "Tax ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Tax");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Tax not found");
        });

        Options(o => o.WithTags(EndpointGroups.Taxes));
    }

    public override async Task HandleAsync(UpdateTaxRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateTaxCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
