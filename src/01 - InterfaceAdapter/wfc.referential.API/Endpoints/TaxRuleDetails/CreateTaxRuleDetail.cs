using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Dtos;

namespace wfc.referential.API.Endpoints.TaxRuleDetails;

public class CreateTaxRuleDetail(IMediator _mediator) : Endpoint<CreateTaxRuleDetailRequest, Guid>
{
    public override void Configure()
    {
        Post("api/taxruledetails");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create tax Rule Detail";
            s.Response<PagedResult<CreateTaxRuleDetailResponse>>(200, "Successful Response");
        });
        Options(o => o.WithTags(EndpointGroups.TaxRuleDetails));
    }

    public override async Task HandleAsync(CreateTaxRuleDetailRequest req, CancellationToken ct)
    {
        var taxCommand = req.Adapt<CreateTaxRuleDetailCommand>();
        var result = await _mediator.Send(taxCommand, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}