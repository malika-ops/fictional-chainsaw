using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Agencies.Commands.UpdateAgency;

public record UpdateAgencyCommand : ICommand<Result<Guid>>
{
    public Guid AgencyId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Fax { get; set; } = string.Empty;
    public string AccountingSheetName { get; set; } = string.Empty;
    public string AccountingAccountNumber { get; set; } = string.Empty;
    public string MoneyGramReferenceNumber { get; set; } = string.Empty;
    public string MoneyGramPassword { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string PermissionOfficeChange { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Guid? CityId { get; set; }
    public Guid? SectorId { get; set; }
    public Guid? AgencyTypeId { get; set; }

    public string? SupportAccountId { get; set; }
    public string? PartnerId { get; set; }
}