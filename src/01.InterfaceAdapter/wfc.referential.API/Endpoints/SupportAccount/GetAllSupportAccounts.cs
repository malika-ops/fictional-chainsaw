using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class GetAllSupportAccounts(IMediator _mediator) : Endpoint<GetAllSupportAccountsRequest, PagedResult<SupportAccountResponse>>
{
    public override void Configure()
    {
        Get("/api/support-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Support Accounts";
            s.Description = "Returns a paginated list of Support Accounts. Supports optional filtering by code, name, threshold, account balance, accounting number, partner ID, and support account type.";

            s.Response<PagedResult<SupportAccountResponse>>(200, "Paged list of Support Accounts");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(GetAllSupportAccountsRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllSupportAccountsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}