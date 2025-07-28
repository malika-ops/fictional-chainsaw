namespace wfc.referential.Application.CurrencyDenominations.Dtos;

public record DeleteCurrencyDenominationRequest
{
    /// <summary>GUID of the currencyDenomination to delete.</summary>
    public Guid CurrencyDenominationId { get; init; }
}
