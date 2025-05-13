using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Agencies.Commands.CreateAgency;

public record CreateAgencyCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Abbreviation { get; init; } = string.Empty;
    public string Address1 { get; init; } = string.Empty;
    public string? Address2 { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string Fax { get; init; } = string.Empty;
    public string AccountingSheetName { get; init; } = string.Empty;
    public string AccountingAccountNumber { get; init; } = string.Empty;
    public string MoneyGramReferenceNumber { get; init; } = string.Empty;
    public string MoneyGramPassword { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string PermissionOfficeChange { get; init; } = string.Empty;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public Guid? CityId { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? AgencyTypeId { get; init; }

    public string? SupportAccountId { get; init; }
    public string? PartnerId { get; init; }
}
