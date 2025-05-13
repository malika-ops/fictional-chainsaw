using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public class PatchParamTypeCommandHandler : ICommandHandler<PatchParamTypeCommand, Result<Guid>>
{
    private readonly IParamTypeRepository _paramTypeRepository;

    public PatchParamTypeCommandHandler(IParamTypeRepository paramTypeRepository)
    {
        _paramTypeRepository = paramTypeRepository;
    }

    public async Task<Result<Guid>> Handle(PatchParamTypeCommand request, CancellationToken cancellationToken)
    {
        var paramType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(request.ParamTypeId), cancellationToken);

        if (paramType == null)
            throw new BusinessException($"ParamType with ID {request.ParamTypeId} not found");

        // Apply value update if provided
        if (request.Value != null)
        {
            paramType.Update(request.Value);
        }

        // Handle enable/disable separately
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !paramType.IsEnabled)
            {
                paramType.Activate();
            }
            else if (!request.IsEnabled.Value && paramType.IsEnabled)
            {
                paramType.Disable();
            }
        }

        // Only call Patch if value was changed
        if (request.Value != null)
        {
            paramType.Patch();
        }

        await _paramTypeRepository.UpdateParamTypeAsync(paramType, cancellationToken);

        return Result.Success(paramType.Id.Value);
    }
}