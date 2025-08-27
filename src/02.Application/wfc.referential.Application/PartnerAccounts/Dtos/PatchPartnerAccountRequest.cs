using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record PatchPartnerAccountRequest
{
    /// <summary>
    /// The ID of the Partner Account to patch.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f6</example>
    public Guid PartnerAccountId { get; init; }

    /// <summary>
    /// If provided, updates the account number. If omitted, account number remains unchanged.
    /// </summary>
    /// <example>000987654321</example>
    public string? AccountNumber { get; init; }

    /// <summary>
    /// If provided, updates the RIB. If omitted, RIB remains unchanged.
    /// </summary>
    /// <example>98765432109876543210987</example>
    public string? RIB { get; init; }

    /// <summary>
    /// If provided, updates the domiciliation. If omitted, domiciliation remains unchanged.
    /// </summary>
    /// <example>Casablanca Marina</example>
    public string? Domiciliation { get; init; }

    /// <summary>
    /// If provided, updates the business name. If omitted, business name remains unchanged.
    /// </summary>
    /// <example>Transfert Express</example>
    public string? BusinessName { get; init; }

    /// <summary>
    /// If provided, updates the short name. If omitted, short name remains unchanged.
    /// </summary>
    /// <example>TE</example>
    public string? ShortName { get; init; }

    /// <summary>
    /// If provided, updates the account balance. If omitted, account balance remains unchanged.
    /// </summary>
    /// <example>75000.00</example>
    public decimal? AccountBalance { get; init; }

    /// <summary>
    /// If provided, updates the bank. If omitted, bank remains unchanged.
    /// </summary>
    /// <example>8b583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public Guid? BankId { get; init; }

    /// <summary>
    /// If provided, updates the account type. If omitted, account type remains unchanged.
    /// </summary>
    /// <example>4a583b69-6e16-5b2c-9c8f-69627ee725d4</example>
    public PartnerAccountTypeEnum? PartnerAccountType { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }
}