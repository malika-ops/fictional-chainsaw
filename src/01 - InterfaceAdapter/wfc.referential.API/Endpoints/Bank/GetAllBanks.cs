using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Banks.Queries.GetAllBanks;

namespace wfc.referential.API.Endpoints.Bank;

public class GetAllBanks(IMediator _mediator) : Endpoint<GetAllBanksRequest, PagedResult<BankResponse>>
{
    public override void Configure()
    {
        Get("/api/banks");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Banks";
            s.Description = "Returns a paginated list of Banks. Supports optional filtering by code, name, abbreviation, and status.";

            s.Response<PagedResult<BankResponse>>(200, "Paged list of Banks");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags("Banks"));
    }

    public override async Task HandleAsync(GetAllBanksRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllBanksQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}