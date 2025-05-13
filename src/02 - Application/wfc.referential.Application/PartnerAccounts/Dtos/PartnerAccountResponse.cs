namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record PartnerAccountResponse(
    Guid PartnerAccountId,
    string AccountNumber,
    string RIB,
    string Domiciliation,
    string BusinessName,
    string ShortName,
    decimal AccountBalance,
    Guid BankId,
    string BankName,
    string BankCode,
    string AccountType,
    bool IsEnabled);