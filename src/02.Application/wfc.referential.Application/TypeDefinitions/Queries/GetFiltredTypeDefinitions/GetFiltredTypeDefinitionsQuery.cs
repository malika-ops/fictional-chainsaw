using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate; 

namespace wfc.referential.Application.TypeDefinitions.Queries.GetFiltredTypeDefinitions;

public record GetFiltredTypeDefinitionsQuery : IQuery<PagedResult<GetFiltredTypeDefinitionsResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Libelle { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; } = true;
    public string CacheKey => $"{nameof(TypeDefinition)}_page{PageNumber}_size{PageSize}_libelle{Libelle}_description{Description}_status{IsEnabled}";
    public int CacheExpiration => 5;
}