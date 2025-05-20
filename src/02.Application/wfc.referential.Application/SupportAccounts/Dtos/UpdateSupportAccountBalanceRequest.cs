using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.SupportAccounts.Dtos;

public record UpdateSupportAccountBalanceRequest
{
    /// <summary>
    /// The ID of the Support Account to update balance.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f7</example>
    public Guid SupportAccountId { get; init; }

    /// <summary>
    /// The new balance for the account.
    /// </summary>
    /// <example>45000.00</example>
    public decimal NewBalance { get; init; }
}