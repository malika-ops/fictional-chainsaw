using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public class UpdateParamTypeCommandHandler(IParamTypeRepository _paramTypeRepository,ICacheService _cacheService) : ICommandHandler<UpdateParamTypeCommand, Guid>
{
    public async Task<Guid> Handle(UpdateParamTypeCommand request, CancellationToken cancellationToken)
    {
        var paramType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(request.ParamTypeId.Value), cancellationToken);

        if (paramType is null)
            throw new BusinessException($"ParamType with ID {request.ParamTypeId.Value} not found");

        // Update value property
        paramType.Update(request.Value,request.IsEnabled);

        await _paramTypeRepository.UpdateParamTypeAsync(paramType, cancellationToken);
        await _paramTypeRepository.SaveChangesAsync(cancellationToken);

        // Remove cache entries with the prefix
        await _cacheService.RemoveByPrefixAsync(CacheKeys.ParamType.Prefix, cancellationToken);

        // Return the ID of the updated ParamType
        return paramType.Id!.Value;
    }
}