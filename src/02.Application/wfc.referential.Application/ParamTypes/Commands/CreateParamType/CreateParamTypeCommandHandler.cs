using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ParamTypeAggregate.Exceptions;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public class CreateParamTypeCommandHandler : ICommandHandler<CreateParamTypeCommand, Result<Guid>>
{
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;
    private readonly ICacheService _cacheService;

    public CreateParamTypeCommandHandler(
        IParamTypeRepository paramTypeRepository,
        ITypeDefinitionRepository typeDefinitionRepository,
        ICacheService cacheService)
    {
        _paramTypeRepository = paramTypeRepository;
        _typeDefinitionRepository = typeDefinitionRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(CreateParamTypeCommand command, CancellationToken ct)
    {
        var typeDefinition = await _typeDefinitionRepository.GetByIdAsync(TypeDefinitionId.Of(command.TypeDefinitionId), ct);
        if (typeDefinition == null)
            throw new BusinessException($"TypeDefinition with ID {command.TypeDefinitionId} not found");

        var existingValueParamType = await _paramTypeRepository.GetOneByConditionAsync(p => p.Value == command.Value, ct);
        if (existingValueParamType is not null)
            throw new ParamTypeValueAlreadyExistException(command.Value);

        var paramType = ParamType.Create(
            ParamTypeId.Of(Guid.NewGuid()),
            TypeDefinitionId.Of(command.TypeDefinitionId),
            command.Value);

        await _paramTypeRepository.AddAsync(paramType, ct);
        await _paramTypeRepository.SaveChangesAsync(ct);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.ParamType.Prefix, ct);

        return Result.Success(paramType.Id!.Value);
    }
}