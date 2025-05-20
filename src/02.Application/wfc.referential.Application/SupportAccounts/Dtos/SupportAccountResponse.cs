namespace wfc.referential.Application.SupportAccounts.Dtos;

public record SupportAccountResponse(
    Guid SupportAccountId,
    string Code,
    string Name,
    decimal Threshold,
    decimal Limit,
    decimal AccountBalance,
    string AccountingNumber,
    Guid PartnerId,
    string PartnerCode,
    string PartnerLabel,
    string SupportAccountType,
    bool IsEnabled);