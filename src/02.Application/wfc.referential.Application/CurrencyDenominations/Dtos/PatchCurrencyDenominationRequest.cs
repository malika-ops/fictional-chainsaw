using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Dtos;

public record PatchCurrencyDenominationRequest
{
    /// <summary>Unique currency id.</summary>
    public Guid? CurrencyId { get; init; }

    /// <summary>Display Type.</summary>
    public CurrencyDenominationType? Type { get; init; }

    /// <summary>Value.</summary>
    public decimal? Value { get; init; }

    /// <summary>Currency denomination status (enabled/disabled).</summary>
    public bool? IsEnabled { get; init; }
}
