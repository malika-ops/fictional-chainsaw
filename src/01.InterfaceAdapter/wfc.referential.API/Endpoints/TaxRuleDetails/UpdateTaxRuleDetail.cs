using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Dtos;

namespace wfc.referential.API.Endpoints.TaxRuleDetails;

public class UpdateTaxRuleDetail(IMediator _mediator) : Endpoint<UpdateTaxRuleDetailRequest, Guid>
{
    public override void Configure()
    {
        Put("api/taxruledetails/{TaxRuleDetailsId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a Tax's properties";
            s.Description = "Updates all fields of the specified Tax ID.";
            s.Params["TaxRuleDetailsId"] = "Tax ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Tax");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Tax not found");
        });

        Options(o => o.WithTags(EndpointGroups.TaxRuleDetails));
    }

    public override async Task HandleAsync(UpdateTaxRuleDetailRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateTaxRuleDetailCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
