namespace wfc.referential.Application.PartnerAccounts.Dtos;

public record GetAllPartnerAccountsRequest
{
    /// <summary>Optional page number (default = 1).</summary>
    public int? PageNumber { get; init; } = 1;

    /// <summary>Optional page size (default = 10).</summary>
    public int? PageSize { get; init; } = 10;

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

    /// <summary>Optional filter by Account Type (Activité/Commission).</summary>
    public string? AccountType { get; init; } = null;

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; } = true;
}