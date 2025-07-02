using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Queries.GetParamTypeById;

public class GetParamTypeByIdQueryHandler : IQueryHandler<GetParamTypeByIdQuery, ParamTypesResponse>
{
    private readonly IParamTypeRepository _paramTypeRepository;

    public GetParamTypeByIdQueryHandler(IParamTypeRepository paramTypeRepository)
    {
        _paramTypeRepository = paramTypeRepository;
    }

    public async Task<ParamTypesResponse> Handle(GetParamTypeByIdQuery query, CancellationToken ct)
    {
        var id = ParamTypeId.Of(query.ParamTypeId);
        var entity = await _paramTypeRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"ParamType with id '{query.ParamTypeId}' not found.");

        return entity.Adapt<ParamTypesResponse>();
    }
} 