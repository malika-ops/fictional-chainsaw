using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Banks.Queries.GetAllBanks;

namespace wfc.referential.API.Endpoints.Bank;

public class GetAllBanksEndpoint(IMediator _mediator)
    : Endpoint<GetAllBanksRequest, PagedResult<GetBanksResponse>>
{
    public override void Configure()
    {
        Get("/api/banks");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of banks";
            s.Description = """
                Returns a paginated list of banks.
                Filters supported: code, name, abbreviation, status.
                """;
            s.Response<PagedResult<GetBanksResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.Banks));
    }

    public override async Task HandleAsync(GetAllBanksRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllBanksQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}