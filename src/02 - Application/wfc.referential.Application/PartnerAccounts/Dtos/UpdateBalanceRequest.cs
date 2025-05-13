using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record UpdateBalanceRequest
{
    /// <summary>
    /// The ID of the Partner Account to update balance.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid PartnerAccountId { get; init; }

    /// <summary>
    /// The new balance for the account.
    /// </summary>
    /// <example>85000.00</example>
    public decimal NewBalance { get; init; }
}