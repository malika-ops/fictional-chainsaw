using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;

public class UpdateTypeDefinitionCommandHandler(ITypeDefinitionRepository _typeDefinitionRepository , ICacheService _cacheService) 
    : ICommandHandler<UpdateTypeDefinitionCommand, Guid>
{
    public async Task<Guid> Handle(UpdateTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var typedefinition = await _typeDefinitionRepository.GetByIdAsync(TypeDefinitionId.Of(request.TypeDefinitionId), cancellationToken);

        if (typedefinition is null)
            throw new BusinessException($"TypeDefinition with ID {request.TypeDefinitionId} not found");

        var isExist = await _typeDefinitionRepository.GetByLibelleAsync(request.Libelle, cancellationToken);
        if (isExist is not null) throw new TypeDefinitionLibelleAlreadyExistException(request.Libelle);

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
        await _typeDefinitionRepository.SaveChangesAsync(cancellationToken);
        
        // Clear cache
        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, cancellationToken);

        return typedefinition.Id!.Value;
    }
}