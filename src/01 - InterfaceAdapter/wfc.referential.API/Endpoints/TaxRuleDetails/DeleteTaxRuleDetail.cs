using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Dtos;

namespace wfc.referential.API.Endpoints.TaxRuleDetails;

public class DeleteTaxRuleDetail(IMediator _mediator) : Endpoint<DeleteTaxRuleDetailRequest, bool>
{
    public override void Configure()
    {
        Delete("api/taxruledetails/{TaxRuleDetailId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a tax rule detail";
        });
        Options(o => o.WithTags(EndpointGroups.TaxRuleDetails));
    }

    public override async Task HandleAsync(DeleteTaxRuleDetailRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteTaxRuleDetailCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}