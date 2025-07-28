using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Dtos;

public record GetFiltredCurrencyDenominationsRequest : FilterRequest
{
    /// <summary>Filter by currency id .</summary>
    public Guid? CurrencyId { get; init; }

    /// <summary>Filter by type.</summary>
    public CurrencyDenominationType? Type { get; init; }

    /// <summary>Filter by value.</summary>
    public decimal? Value { get; init; }
}