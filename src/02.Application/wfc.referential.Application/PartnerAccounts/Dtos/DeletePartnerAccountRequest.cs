namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record DeletePartnerAccountRequest
{
    /// <summary>
    /// The ID of the Partner Account to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid PartnerAccountId { get; init; }
}