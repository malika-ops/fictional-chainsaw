using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;

namespace wfc.referential.Application.ParamTypes.Commands.CreateParamType;

public class CreateParamTypeCommandHandler : ICommandHandler<CreateParamTypeCommand, Result<Guid>>
{
    private readonly IParamTypeRepository _paramTypeRepository;
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;

    public CreateParamTypeCommandHandler(
        IParamTypeRepository paramTypeRepository,
        ITypeDefinitionRepository typeDefinitionRepository)
    {
        _paramTypeRepository = paramTypeRepository;
        _typeDefinitionRepository = typeDefinitionRepository;
    }

    public async Task<Result<Guid>> Handle(CreateParamTypeCommand request, CancellationToken cancellationToken)
    {
        var typeDefinition = await _typeDefinitionRepository.GetByIdAsync(request.TypeDefinitionId, cancellationToken);
        if (typeDefinition == null)
            throw new BusinessException($"TypeDefinition with ID {request.TypeDefinitionId.Value} not found");

        var paramType = ParamType.Create(request.ParamTypeId, request.TypeDefinitionId, request.Value);

        await _paramTypeRepository.AddParamTypeAsync(paramType, cancellationToken);

        return Result.Success(paramType.Id.Value);
    }
}