using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;

public class DeleteTypeDefinitionCommandHandler : ICommandHandler<DeleteTypeDefinitionCommand, bool>
{
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;

    public DeleteTypeDefinitionCommandHandler(ITypeDefinitionRepository typeDefinitionRepository)
    {
        _typeDefinitionRepository = typeDefinitionRepository;
    }

    public async Task<bool> Handle(DeleteTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var typedefinition = await _typeDefinitionRepository.GetByIdAsync(TypeDefinitionId.Of(request.TypeDefinitionId), cancellationToken);

        if (typedefinition == null)
            throw new BusinessException("Type definition not found");

        if (typedefinition.ParamTypes.Count > 0)
            throw new TypeDefinitionLinkedToParamTypeException(request.TypeDefinitionId);

        typedefinition.Disable();

        await _typeDefinitionRepository.UpdateTypeDefinitionAsync(typedefinition, cancellationToken);

        return true;
    }
}