namespace wfc.referential.Application.Banks.Dtos;

public record BankResponse(
    Guid BankId,
    string Code,
    string Name,
    string Abbreviation,
    bool IsEnabled);