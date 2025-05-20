using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Caching.Interface;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;

public record GetAllTypeDefinitionsQuery(int PageNumber, int PageSize, string? Libelle, string? Description, bool? IsEnabled) 
    : IQuery<PagedResult<GetAllTypeDefinitionsResponse>>, ICacheableQuery
{
    public string CacheKey => $"{nameof(TypeDefinition)}_page{PageNumber}_size{PageSize}_libelle{Libelle}_description{Description}_status{IsEnabled}";
    public int CacheExpiration => 5;

}