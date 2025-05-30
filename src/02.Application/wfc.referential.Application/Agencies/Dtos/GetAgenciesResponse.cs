namespace wfc.referential.Application.Agencies.Dtos;

public record GetAgenciesResponse
{

    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;

    public string Address1 { get; init; } = string.Empty;
    public string? Address2 { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string Fax { get; init; } = string.Empty;

    public string AccountingSheetName { get; init; } = string.Empty;
    public string AccountingAccountNumber { get; init; } = string.Empty;
    public string? ExpenseFundAccountingSheet { get; init; }
    public string? ExpenseFundAccountNumber { get; init; }
    public string? MadAccount { get; init; }

    public string PostalCode { get; init; } = string.Empty;
    public string? CashTransporter { get; init; }
    public decimal? FundingThreshold { get; init; }

    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public Guid? CityId { get; init; }
    public Guid? SectorId { get; init; }

    public Guid? AgencyTypeId { get; init; }
    public string? AgencyTypeLibelle { get; init; }
    public string? AgencyTypeValue { get; init; }

    public Guid? FundingTypeId { get; init; }
    public Guid? TokenUsageStatusId { get; init; }

    public Guid? PartnerId { get; init; }
    public Guid? SupportAccountId { get; init; }

    public bool IsEnabled { get; init; }

};
