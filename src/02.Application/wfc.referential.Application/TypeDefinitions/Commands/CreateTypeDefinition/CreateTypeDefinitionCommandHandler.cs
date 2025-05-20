using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public class CreateTypeDefinitionCommandHandler(ITypeDefinitionRepository _typeDefinitionRepository,ICacheService _cacheService) 
    : ICommandHandler<CreateTypeDefinitionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var isExist = await _typeDefinitionRepository.GetByLibelleAsync(request.Libelle, cancellationToken);
        if (isExist is not null) throw new TypeDefinitionLibelleAlreadyExistException(request.Libelle);

        var id = TypeDefinitionId.Of(Guid.NewGuid());
        var typedefinition = TypeDefinition.Create(id, request.Libelle, request.Description, []);

        await _typeDefinitionRepository.AddTypeDefinitionAsync(typedefinition, cancellationToken);

        await _typeDefinitionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, cancellationToken);

        return Result.Success(typedefinition.Id!.Value);
    }
}