namespace wfc.referential.Application.Currencies.Dtos;

public record DeleteCurrencyRequest
{
    /// <summary>GUID of the currency to delete.</summary>
    public Guid CurrencyId { get; init; }
}
