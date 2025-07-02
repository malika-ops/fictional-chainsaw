using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.Application.Currencies.Queries.GetCurrencyById;

public record GetCurrencyByIdQuery : IQuery<GetCurrenciesResponse>
{
    public Guid CurrencyId { get; init; }
} 