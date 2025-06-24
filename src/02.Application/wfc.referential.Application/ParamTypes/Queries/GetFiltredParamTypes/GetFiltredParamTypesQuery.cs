using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Queries.GetFiltredParamTypes;

public record GetFiltredParamTypesQuery : IQuery<PagedResult<GetFiltredParamTypesResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Value { get; init; }
    public TypeDefinitionId TypeDefinitionId { get; init; }
    public bool? IsEnabled { get; init; }

    public string CacheKey => $"{nameof(TypeDefinition)}_page{PageNumber}_size{PageSize}_value{Value}_status{IsEnabled}";
    public int CacheExpiration => 5;
}