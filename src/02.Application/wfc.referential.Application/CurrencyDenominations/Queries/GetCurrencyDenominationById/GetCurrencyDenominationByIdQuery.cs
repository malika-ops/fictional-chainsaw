using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.CurrencyDenominations.Dtos;

namespace wfc.referential.Application.CurrencyDenominations.Queries.GetCurrencyDenominationById;

public record GetCurrencyDenominationByIdQuery : IQuery<GetCurrencyDenominationsResponse>
{
    public Guid CurrencyDenominationId { get; init; }
} 