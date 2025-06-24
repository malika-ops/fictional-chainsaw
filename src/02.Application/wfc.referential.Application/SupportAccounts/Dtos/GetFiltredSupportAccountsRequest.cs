namespace wfc.referential.Application.SupportAccounts.Dtos;

public record GetFiltredSupportAccountsRequest : FilterRequest
{
    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional minimum threshold.</summary>
    public decimal? MinThreshold { get; init; }

    /// <summary>Optional maximum threshold.</summary>
    public decimal? MaxThreshold { get; init; }

    /// <summary>Optional minimum limit.</summary>
    public decimal? MinLimit { get; init; }

    /// <summary>Optional maximum limit.</summary>
    public decimal? MaxLimit { get; init; }

    /// <summary>Optional minimum account balance.</summary>
    public decimal? MinAccountBalance { get; init; }

    /// <summary>Optional maximum account balance.</summary>
    public decimal? MaxAccountBalance { get; init; }

    /// <summary>Optional filter by accounting number.</summary>
    public string? AccountingNumber { get; init; }

    /// <summary>Optional filter by Partner ID.</summary>
    public Guid? PartnerId { get; init; }

    /// <summary>Optional filter by Support Account Type value.</summary>
    public string? SupportAccountType { get; init; } = null;
}