using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.DeleteParamType;

public class DeleteParamTypeCommandHandler(IParamTypeRepository paramTypeRepository) : ICommandHandler<DeleteParamTypeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteParamTypeCommand request, CancellationToken cancellationToken)
    {
        var paramtype = await paramTypeRepository.GetByIdAsync(ParamTypeId.Of(request.ParamTypeId), cancellationToken);

        if (paramtype is null)
            throw new ResourceNotFoundException($"{nameof(ParamType)} not found");

        paramtype.Disable();

        await paramTypeRepository.UpdateParamTypeAsync(paramtype, cancellationToken);

        return Result.Success(true);

    }
}
