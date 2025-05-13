using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.UpdateParamType;

public class UpdateParamTypeCommandHandler : ICommandHandler<UpdateParamTypeCommand, Guid>
{
    private readonly IParamTypeRepository _paramTypeRepository;

    public UpdateParamTypeCommandHandler(IParamTypeRepository paramTypeRepository)
    {
        _paramTypeRepository = paramTypeRepository;
    }

    public async Task<Guid> Handle(UpdateParamTypeCommand request, CancellationToken cancellationToken)
    {
        var paramType = await _paramTypeRepository.GetByIdAsync(ParamTypeId.Of(request.ParamTypeId.Value), cancellationToken);

        if (paramType is null)
            throw new BusinessException($"ParamType with ID {request.ParamTypeId.Value} not found");

        // Update value property
        paramType.Update(request.Value);

        // Handle enable/disable separately
        if (request.IsEnabled && !paramType.IsEnabled)
        {
            paramType.Activate();
        }
        else if (!request.IsEnabled && paramType.IsEnabled)
        {
            paramType.Disable();
        }

        await _paramTypeRepository.UpdateParamTypeAsync(paramType, cancellationToken);

        return paramType.Id!.Value;
    }
}