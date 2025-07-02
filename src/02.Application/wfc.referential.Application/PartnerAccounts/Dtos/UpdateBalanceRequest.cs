namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record UpdateBalanceRequest
{
    /// <summary>
    /// The new balance for the account.
    /// </summary>
    /// <example>85000.00</example>
    public decimal NewBalance { get; init; }
}