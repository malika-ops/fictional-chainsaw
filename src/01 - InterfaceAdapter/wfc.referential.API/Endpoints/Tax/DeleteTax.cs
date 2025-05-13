using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Taxes.Commands.DeleteTax;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.API.Endpoints.Tax;

public class DeleteTax(IMediator _mediator) : Endpoint<DeleteTaxRequest, bool>
{
    public override void Configure()
    {
        Delete("api/taxes/{TaxId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a tax";
        });
        Options(o => o.WithTags(EndpointGroups.Taxes));
    }

    public override async Task HandleAsync(DeleteTaxRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteTaxCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}