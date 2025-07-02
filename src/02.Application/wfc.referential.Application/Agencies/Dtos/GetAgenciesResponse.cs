namespace wfc.referential.Application.Agencies.Dtos;

public record GetAgenciesResponse
{
    /// <summary>
    /// Unique identifier of the agency.
    /// </summary>
    /// <example>e3b0c442-98fc-1c14-9afb-4c1a1e1f2a3b</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Unique code of the agency.
    /// </summary>
    /// <example>AGY001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Name of the agency.
    /// </summary>
    /// <example>Central Agency</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Abbreviation of the agency name.
    /// </summary>
    /// <example>CENTRAL</example>
    public string Abbreviation { get; init; } = string.Empty;

    /// <summary>
    /// Primary address of the agency.
    /// </summary>
    /// <example>123 Main St</example>
    public string Address1 { get; init; } = string.Empty;

    /// <summary>
    /// Secondary address of the agency.
    /// </summary>
    /// <example>Suite 400</example>
    public string? Address2 { get; init; }

    /// <summary>
    /// Phone number of the agency.
    /// </summary>
    /// <example>+1-555-1234</example>
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Fax number of the agency.
    /// </summary>
    /// <example>+1-555-5678</example>
    public string Fax { get; init; } = string.Empty;

    /// <summary>
    /// Name of the accounting sheet for the agency.
    /// </summary>
    /// <example>AGENCY_SHEET</example>
    public string AccountingSheetName { get; init; } = string.Empty;

    /// <summary>
    /// Accounting account number for the agency.
    /// </summary>
    /// <example>401200</example>
    public string AccountingAccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// Name of the accounting sheet for expense funds.
    /// </summary>
    /// <example>EXPENSE_SHEET</example>
    public string? ExpenseFundAccountingSheet { get; init; }

    /// <summary>
    /// Account number for expense funds.
    /// </summary>
    /// <example>512300</example>
    public string? ExpenseFundAccountNumber { get; init; }

    /// <summary>
    /// MAD (Moroccan Dirham) account number.
    /// </summary>
    /// <example>MA123456789</example>
    public string? MadAccount { get; init; }

    /// <summary>
    /// Postal code of the agency's location.
    /// </summary>
    /// <example>10000</example>
    public string PostalCode { get; init; } = string.Empty;

    /// <summary>
    /// Name of the cash transporter for the agency.
    /// </summary>
    /// <example>SecureTrans</example>
    public string? CashTransporter { get; init; }

    /// <summary>
    /// Funding threshold for the agency.
    /// </summary>
    /// <example>5000.00</example>
    public decimal? FundingThreshold { get; init; }

    /// <summary>
    /// Latitude coordinate of the agency.
    /// </summary>
    /// <example>34.020882</example>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Longitude coordinate of the agency.
    /// </summary>
    /// <example>-6.841650</example>
    public decimal? Longitude { get; init; }

    /// <summary>
    /// Unique identifier of the city where the agency is located.
    /// </summary>
    /// <example>f7e6d5c4-b3a2-1098-7654-3210fedcba98</example>
    public Guid? CityId { get; init; }

    /// <summary>
    /// Unique identifier of the sector where the agency is located.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid? SectorId { get; init; }

    /// <summary>
    /// Unique identifier of the agency type.
    /// </summary>
    /// <example>e1d2c3b4-a5f6-7890-bcde-1234567890ab</example>
    public Guid? AgencyTypeId { get; init; }

    /// <summary>
    /// Label of the agency type.
    /// </summary>
    /// <example>Branch</example>
    public string? AgencyTypeLibelle { get; init; }

    /// <summary>
    /// Value of the agency type.
    /// </summary>
    /// <example>BRANCH</example>
    public string? AgencyTypeValue { get; init; }

    /// <summary>
    /// Unique identifier of the funding type.
    /// </summary>
    /// <example>f1e2d3c4-b5a6-7890-1234-56789abcdef0</example>
    public Guid? FundingTypeId { get; init; }

    /// <summary>
    /// Unique identifier of the token usage status.
    /// </summary>
    /// <example>0a1b2c3d-4e5f-6789-abcd-ef0123456789</example>
    public Guid? TokenUsageStatusId { get; init; }

    /// <summary>
    /// Unique identifier of the partner associated with the agency.
    /// </summary>
    /// <example>c9d8e7f6-a5b4-3210-9876-54321fedcba0</example>
    public Guid? PartnerId { get; init; }

    /// <summary>
    /// Unique identifier of the support account.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid? SupportAccountId { get; init; }

    /// <summary>
    /// Indicates whether the agency is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }
}
