using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Taxes.Commands.PatchTax;
using wfc.referential.Application.Taxes.Dtos;


namespace wfc.referential.API.Endpoints.Tax;

public class PatchTax(IMediator _mediator) : Endpoint<PatchTaxRequest, Guid>
{
    public override void Configure()
    {
        Patch("api/taxes/{TaxId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Tax's properties";
            s.Description = "Updates only the provided fields (code, name, statu or countryId) of the specified Tax ID.";
            s.Params["TaxId"] = "Tax ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Tax");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Tax not found");
        });

        Options(o => o.WithTags(EndpointGroups.Taxes));
    }

    public override async Task HandleAsync(PatchTaxRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchTaxCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
