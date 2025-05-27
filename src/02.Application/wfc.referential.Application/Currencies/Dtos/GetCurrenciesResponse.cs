namespace wfc.referential.Application.Currencies.Dtos;

public record GetCurrenciesResponse
{
    public Guid CurrencyId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string CodeAR { get; init; } = string.Empty;
    public string CodeEN { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int CodeIso { get; init; }
    public bool IsEnabled { get; init; }
}