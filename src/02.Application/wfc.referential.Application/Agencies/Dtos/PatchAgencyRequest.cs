using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.Agencies.Dtos;

public record PatchAgencyRequest
{
    /// <summary>Agency GUID (route).</summary>
    [Required] public Guid AgencyId { get; init; }

    /// <summary>6-digit agency code (must stay unique).</summary>
    public string? Code { get; init; }

    /// <summary>Display name.</summary>              
    public string? Name { get; init; }
    /// <summary>Short abbreviation.</summary>        
    public string? Abbreviation { get; init; }
    /// <summary>Primary address line.</summary>      
    public string? Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }

    public string? AccountingSheetName { get; init; }
    public string? AccountingAccountNumber { get; init; }
    public string? ExpenseFundAccountingSheet { get; init; }
    public string? ExpenseFundAccountNumber { get; init; }
    public string? MadAccount { get; init; }

    public string? PostalCode { get; init; }
    public string? CashTransporter { get; init; }
    public decimal? FundingThreshold { get; init; }

    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public Guid? CityId { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? AgencyTypeId { get; init; }
    public Guid? FundingTypeId { get; init; }
    public Guid? TokenUsageStatusId { get; init; }
    public Guid? PartnerId { get; init; }
    public Guid? SupportAccountId { get; init; }

    public bool? IsEnabled { get; init; }
}
