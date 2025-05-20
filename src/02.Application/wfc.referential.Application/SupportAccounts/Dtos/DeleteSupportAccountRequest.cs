namespace wfc.referential.Application.SupportAccounts.Dtos;

public record DeleteSupportAccountRequest
{
    /// <summary>
    /// The ID of the Support Account to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f7</example>
    public Guid SupportAccountId { get; init; }
}