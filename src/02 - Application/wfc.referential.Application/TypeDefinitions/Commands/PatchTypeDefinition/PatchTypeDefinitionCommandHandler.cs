using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public class PatchTypeDefinitionCommandHandler : ICommandHandler<PatchTypeDefinitionCommand, Result<Guid>>
{
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;

    public PatchTypeDefinitionCommandHandler(ITypeDefinitionRepository typeDefinitionRepository)
    {
        _typeDefinitionRepository = typeDefinitionRepository;
    }

    public async Task<Result<Guid>> Handle(PatchTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var typeDefinition = await _typeDefinitionRepository.GetByIdAsync(TypeDefinitionId.Of(request.TypeDefinitionId), cancellationToken);
        if (typeDefinition == null)
            throw new BusinessException($"TypeDefinition with ID {request.TypeDefinitionId} not found");

        // Check if basic properties need update
        bool basicPropertiesChanged = false;
        if (request.Libelle != null || request.Description != null)
        {
            var updatedLibelle = request.Libelle ?? typeDefinition.Libelle;
            var updatedDescription = request.Description ?? typeDefinition.Description;

            // Only call update if there's actually a change
            if (updatedLibelle != typeDefinition.Libelle || updatedDescription != typeDefinition.Description)
            {
                typeDefinition.Update(updatedLibelle, updatedDescription);
                basicPropertiesChanged = true;
            }
        }

        // Handle enable/disable separately
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !typeDefinition.IsEnabled)
            {
                typeDefinition.Activate();
            }
            else if (!request.IsEnabled.Value && typeDefinition.IsEnabled)
            {
                typeDefinition.Disable();
            }
        }

        // Only call Patch if we made basic property changes
        if (basicPropertiesChanged)
        {
            typeDefinition.Patch();
        }

        await _typeDefinitionRepository.UpdateTypeDefinitionAsync(typeDefinition, cancellationToken);

        return Result.Success(typeDefinition.Id.Value);
    }
}