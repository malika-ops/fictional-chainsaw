using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Application.PartnerAccounts.Queries.GetAllPartnerAccounts;

namespace wfc.referential.API.Endpoints.PartnerAccount;

public class GetAllPartnerAccountsEndpoint(IMediator _mediator) : Endpoint<GetAllPartnerAccountsRequest, PagedResult<PartnerAccountResponse>>
{
    public override void Configure()
    {
        Get("/api/partner-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of partner accounts";
            s.Description = """
                Returns a paginated list of partner accounts.
                Filters supported: accountNumber, RIB, businessName, shortName, balance range, bankId, accountTypeId, status.
                """;
            s.Response<PagedResult<PartnerAccountResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.PartnerAccounts));
    }

    public override async Task HandleAsync(GetAllPartnerAccountsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllPartnerAccountsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}