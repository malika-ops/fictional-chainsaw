using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ParamTypeAggregate.Exceptions;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public class CreateParamTypeCommandHandler(IParamTypeRepository _paramTypeRepository, ITypeDefinitionRepository _typeDefinitionRepository, ICacheService _cacheService) 
    : ICommandHandler<CreateParamTypeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateParamTypeCommand request, CancellationToken cancellationToken)
    {
        var typeDefinition = await _typeDefinitionRepository.GetByIdAsync(request.TypeDefinitionId, cancellationToken);
        if (typeDefinition == null)
            throw new BusinessException($"TypeDefinition with ID {request.TypeDefinitionId.Value} not found");

        var existingValueParamType = await _paramTypeRepository.GetByValueAsync(request.Value, cancellationToken);
        if (existingValueParamType != null)
            throw new ParamTypeValueAlreadyExistException($"ParamType with value {request.Value} already exists");

        var paramType = ParamType.Create(request.ParamTypeId, request.TypeDefinitionId, request.Value);

        await _paramTypeRepository.AddParamTypeAsync(paramType, cancellationToken);
        await _paramTypeRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.ParamType.Prefix,cancellationToken);

        return Result.Success(paramType.Id!.Value);
    }
}