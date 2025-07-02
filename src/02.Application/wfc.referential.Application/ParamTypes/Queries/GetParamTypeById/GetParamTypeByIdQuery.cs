using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.Application.ParamTypes.Queries.GetParamTypeById;

public record GetParamTypeByIdQuery : IQuery<ParamTypesResponse>
{
    public Guid ParamTypeId { get; init; }
} 