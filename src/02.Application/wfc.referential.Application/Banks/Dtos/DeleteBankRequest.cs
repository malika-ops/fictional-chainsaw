namespace wfc.referential.Application.Banks.Dtos;

public record DeleteBankRequest
{
    /// <summary>
    /// The ID of the Bank to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid BankId { get; init; }
}
