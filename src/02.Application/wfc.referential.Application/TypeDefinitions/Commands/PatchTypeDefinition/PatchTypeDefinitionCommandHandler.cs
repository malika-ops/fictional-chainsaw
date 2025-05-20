using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public class PatchTypeDefinitionCommandHandler(ITypeDefinitionRepository _typeDefinitionRepository,ICacheService _cacheService) 
    : ICommandHandler<PatchTypeDefinitionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(PatchTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var typeDefinition = await _typeDefinitionRepository.GetByIdAsync(TypeDefinitionId.Of(request.TypeDefinitionId), cancellationToken);
        if (typeDefinition == null)
            throw new BusinessException($"TypeDefinition with ID {request.TypeDefinitionId} not found");

        if (!string.IsNullOrEmpty(request.Libelle))
        {
            var isExist = await _typeDefinitionRepository.GetByLibelleAsync(request.Libelle, cancellationToken);
            if (isExist is not null) throw new TypeDefinitionLibelleAlreadyExistException(request.Libelle);
        }


        typeDefinition.Patch(request.Libelle , request.Description , request.IsEnabled);

        await _typeDefinitionRepository.UpdateTypeDefinitionAsync(typeDefinition, cancellationToken);
        await _typeDefinitionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, cancellationToken);

        return Result.Success(typeDefinition.Id!.Value);
    }
}