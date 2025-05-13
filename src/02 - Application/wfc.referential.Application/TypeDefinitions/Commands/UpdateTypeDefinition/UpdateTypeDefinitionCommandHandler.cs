using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;

public class UpdateTypeDefinitionCommandHandler : ICommandHandler<UpdateTypeDefinitionCommand, Guid>
{
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;

    public UpdateTypeDefinitionCommandHandler(ITypeDefinitionRepository typeDefinitionRepository)
    {
        _typeDefinitionRepository = typeDefinitionRepository;
    }

    public async Task<Guid> Handle(UpdateTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var typedefinition = await _typeDefinitionRepository.GetByIdAsync(TypeDefinitionId.Of(request.TypeDefinitionId), cancellationToken);

        if (typedefinition is null)
            throw new BusinessException($"TypeDefinition with ID {request.TypeDefinitionId} not found");

        // Update basic properties
        typedefinition.Update(request.Libelle, request.Description);

        // Handle enable/disable separately
        if (request.IsEnabled && !typedefinition.IsEnabled)
        {
            typedefinition.Activate();
        }
        else if (!request.IsEnabled && typedefinition.IsEnabled)
        {
            typedefinition.Disable();
        }

        await _typeDefinitionRepository.UpdateTypeDefinitionAsync(typedefinition, cancellationToken);

        return typedefinition.Id.Value;
    }
}