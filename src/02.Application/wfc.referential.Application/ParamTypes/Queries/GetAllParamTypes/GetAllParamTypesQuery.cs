using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;

public record GetAllParamTypesQuery(int PageNumber, int PageSize, string? Value, TypeDefinitionId TypeDefinitionId,
   bool? IsEnabled) : IQuery<PagedResult<GetAllParamTypesResponse>>
{
    public string CacheKey => $"{nameof(TypeDefinition)}_page{PageNumber}_size{PageSize}_value{Value}_status{IsEnabled}";
    public int CacheExpiration => 5;
}