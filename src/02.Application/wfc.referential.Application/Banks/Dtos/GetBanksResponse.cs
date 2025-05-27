namespace wfc.referential.Application.Banks.Dtos;

public record GetBanksResponse
{
    public Guid BankId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}