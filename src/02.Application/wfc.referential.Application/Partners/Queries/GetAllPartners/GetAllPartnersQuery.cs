using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Queries.GetAllPartners;

public record GetAllPartnersQuery : IQuery<PagedResult<GetPartnersResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? PersonType { get; init; }
    public string? ProfessionalTaxNumber { get; init; }
    public string? HeadquartersCity { get; init; }
    public string? TaxIdentificationNumber { get; init; }
    public string? ICE { get; init; }
    public bool? IsEnabled { get; init; } = true;
}