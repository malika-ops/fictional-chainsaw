namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record PartnerAccountResponse
{
    public Guid PartnerAccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string RIB { get; init; } = string.Empty;
    public string? Domiciliation { get; init; }
    public string? BusinessName { get; init; }
    public string? ShortName { get; init; }
    public decimal AccountBalance { get; init; }
    public Guid BankId { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string BankCode { get; init; } = string.Empty;
    public Guid AccountTypeId { get; init; }
    public string AccountTypeName { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}