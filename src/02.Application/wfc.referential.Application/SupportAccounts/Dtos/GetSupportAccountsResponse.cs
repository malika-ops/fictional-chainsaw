namespace wfc.referential.Application.SupportAccounts.Dtos;

public record GetSupportAccountsResponse
{
    public Guid SupportAccountId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Threshold { get; init; }
    public decimal Limit { get; init; }
    public decimal AccountBalance { get; init; }
    public string AccountingNumber { get; init; } = string.Empty;
    public Guid? PartnerId { get; init; }
    public Guid? SupportAccountTypeId { get; init; }
    public bool IsEnabled { get; init; }
}