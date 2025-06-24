using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public class CreateTypeDefinitionCommandHandler : ICommandHandler<CreateTypeDefinitionCommand, Result<Guid>>
{
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;
    private readonly ICacheService _cacheService;

    public CreateTypeDefinitionCommandHandler(ITypeDefinitionRepository typeDefinitionRepository, ICacheService cacheService)
    {
        _typeDefinitionRepository = typeDefinitionRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(CreateTypeDefinitionCommand command, CancellationToken ct)
    {
        var existingByLibelle = await _typeDefinitionRepository.GetOneByConditionAsync(t => t.Libelle == command.Libelle, ct);
        if (existingByLibelle is not null)
            throw new TypeDefinitionLibelleAlreadyExistException(command.Libelle);

        var typeDefinition = TypeDefinition.Create(
            TypeDefinitionId.Of(Guid.NewGuid()),
            command.Libelle,
            command.Description);

        await _typeDefinitionRepository.AddAsync(typeDefinition, ct);
        await _typeDefinitionRepository.SaveChangesAsync(ct);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, ct);

        return Result.Success(typeDefinition.Id!.Value);
    }
}