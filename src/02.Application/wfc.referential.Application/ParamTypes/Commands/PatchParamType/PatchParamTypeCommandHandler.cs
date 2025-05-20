using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public class PatchParamTypeCommandHandler(IParamTypeRepository _paramTypeRepository,ICacheService _cacheService) : ICommandHandler<PatchParamTypeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchParamTypeCommand request, CancellationToken cancellationToken)
    {
        var paramType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(request.ParamTypeId), cancellationToken);

        if (paramType == null)
            throw new BusinessException($"ParamType with ID {request.ParamTypeId} not found");
            
        paramType.Patch(request.Value,request.IsEnabled);

        await _paramTypeRepository.UpdateParamTypeAsync(paramType, cancellationToken);
        await _paramTypeRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.ParamType.Prefix, cancellationToken);

        return Result.Success(paramType.Id!.Value);
    }
}