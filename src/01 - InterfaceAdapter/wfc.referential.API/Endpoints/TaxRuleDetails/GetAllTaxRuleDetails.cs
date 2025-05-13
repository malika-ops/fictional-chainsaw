using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.TaxRuleDetails.Queries.GetAllTaxeRuleDetails;

namespace wfc.referential.API.Endpoints.TaxRuleDetails;

public class GetAllTaxRuleDetails(IMediator _mediator) : Endpoint<GetAllTaxRuleDetailsRequest, PagedResult<GetAllTaxRuleDetailsResponse>>
{
    public override void Configure()
    {
        Get("api/taxruledetails");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of tax rule details";
            s.Response<PagedResult<GetAllTaxRuleDetailsResponse>>(StatusCodes.Status200OK, "Successful Response");
            s.Response<BadRequest>(StatusCodes.Status400BadRequest,"If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response<InternalErrorResponse>(StatusCodes.Status500InternalServerError, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.TaxRuleDetails));
    }

    public override async Task HandleAsync(GetAllTaxRuleDetailsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllTaxRuleDetailsQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}