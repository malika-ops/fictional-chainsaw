using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Currencies.Queries;

namespace wfc.referential.API.Endpoints.Currency;

public class GetAllCurrencies(IMediator _mediator) : Endpoint<GetAllCurrenciesRequest, PagedResult<CurrencyResponse>>
{
    public override void Configure()
    {
        Get("/api/currencies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Currencies";
            s.Description = "Returns a paginated list of Currencies. Supports optional filtering by code, name, and status.";
            s.Response<PagedResult<CurrencyResponse>>(200, "Paged list of Currencies");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(GetAllCurrenciesRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllCurrenciesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}