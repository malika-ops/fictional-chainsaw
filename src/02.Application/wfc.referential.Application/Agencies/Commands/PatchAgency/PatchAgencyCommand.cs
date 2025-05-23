using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;


namespace wfc.referential.Application.Agencies.Commands.PatchAgency;

public record PatchAgencyCommand : ICommand<Result<bool>>
{
    public Guid AgencyId { get; init; }

    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Abbreviation { get; init; }
    public string? Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? AccountingSheetName { get; init; }
    public string? AccountingAccountNumber { get; init; }
    public string? MoneyGramReferenceNumber { get; init; }
    public string? MoneyGramPassword { get; init; }
    public string? PostalCode { get; init; }
    public string? PermissionOfficeChange { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public bool? IsEnabled { get; init; }

    public Guid? CityId { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? AgencyTypeId { get; init; }

    public string? SupportAccountId { get; init; }
    public string? PartnerId { get; init; }
}