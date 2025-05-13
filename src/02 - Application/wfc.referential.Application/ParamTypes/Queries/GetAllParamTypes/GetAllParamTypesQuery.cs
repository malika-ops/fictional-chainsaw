using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;

public record GetAllParamTypesQuery : IQuery<PagedResult<GetAllParamTypesResponse>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public string? Value { get; init; }
    public TypeDefinitionId TypeDefinitionId { get; init; }
    public bool? IsEnabled { get; init; }

    public GetAllParamTypesQuery(
        int pageNumber,
        int pageSize,
        string? value,
        TypeDefinitionId typeDefinitionId,
        bool? isEnabled = true)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Value = value;
        TypeDefinitionId = typeDefinitionId;
        IsEnabled = isEnabled;
    }
}