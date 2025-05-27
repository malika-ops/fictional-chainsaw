using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Application.SupportAccounts.Queries.GetAllSupportAccounts;

namespace wfc.referential.API.Endpoints.SupportAccount;

public class GetAllSupportAccountsEndpoint(IMediator _mediator)
    : Endpoint<GetAllSupportAccountsRequest, PagedResult<GetSupportAccountsResponse>>
{
    public override void Configure()
    {
        Get("/api/support-accounts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of support accounts";
            s.Description = """
                Returns a paginated list of support accounts.
                Filters supported: code, description, accounting number, partner ID, support account type ID, status.
                """;
            s.Response<PagedResult<GetSupportAccountsResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.SupportAccounts));
    }

    public override async Task HandleAsync(GetAllSupportAccountsRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllSupportAccountsQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}