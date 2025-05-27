using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Currencies.Queries.GetAllCurrencies;

namespace wfc.referential.API.Endpoints.Currency;

public class GetAllCurrenciesEndpoint(IMediator _mediator)
    : Endpoint<GetAllCurrenciesRequest, PagedResult<GetCurrenciesResponse>>
{
    public override void Configure()
    {
        Get("/api/currencies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of currencies";
            s.Description = """
                Returns a paginated list of currencies.
                Filters supported: code, codeAR, codeEN, name, codeIso, status.
                """;
            s.Response<PagedResult<GetCurrenciesResponse>>(200, "Successful response");
            s.Response(400, "Invalid pagination/filter parameters");
            s.Response(500, "Server error");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(GetAllCurrenciesRequest req, CancellationToken ct)
    {
        var query = req.Adapt<GetAllCurrenciesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}