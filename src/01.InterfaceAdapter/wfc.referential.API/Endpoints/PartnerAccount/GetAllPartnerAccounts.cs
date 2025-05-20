using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class GetAllPartnerAccounts(IMediator _mediator) : Endpoint<GetAllPartnerAccountsRequest, PagedResult<PartnerAccountResponse>>
{
    public override void Configure()
    {
        Get("/api/partner-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Partner Accounts";
            s.Description = "Returns a paginated list of Partner Accounts. Supports optional filtering by account number, RIB, business name, short name, balance range, bank ID, and account type.";

            s.Response<PagedResult<PartnerAccountResponse>>(200, "Paged list of Partner Accounts");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags("PartnerAccounts"));
    }

    public override async Task HandleAsync(GetAllPartnerAccountsRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllPartnerAccountsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}