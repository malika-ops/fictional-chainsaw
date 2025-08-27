using wfc.referential.Domain.PartnerAccountAggregate;

namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record GetFiltredPartnerAccountsRequest : FilterRequest
{
    /// <summary>Optional filter by account number.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Optional filter by RIB.</summary>
    public string? RIB { get; init; }

    /// <summary>Optional filter by business name.</summary>
    public string? BusinessName { get; init; }

    /// <summary>Optional filter by short name.</summary>
    public string? ShortName { get; init; }

    /// <summary>Optional minimum account balance.</summary>
    public decimal? MinAccountBalance { get; init; }

    /// <summary>Optional maximum account balance.</summary>
    public decimal? MaxAccountBalance { get; init; }

    /// <summary>Optional filter by Bank ID.</summary>
    public Guid? BankId { get; init; }

    /// <summary>Optional filter by Account Type.</summary>
    public PartnerAccountTypeEnum? PartnerAccountType { get; init; }
}