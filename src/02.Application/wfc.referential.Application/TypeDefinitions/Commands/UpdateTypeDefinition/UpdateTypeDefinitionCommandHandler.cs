using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;

public class UpdateTypeDefinitionCommandHandler : ICommandHandler<UpdateTypeDefinitionCommand, Result<bool>>
{
    private readonly ITypeDefinitionRepository _repo;
    private readonly ICacheService _cacheService;

    public UpdateTypeDefinitionCommandHandler(ITypeDefinitionRepository repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(UpdateTypeDefinitionCommand cmd, CancellationToken ct)
    {
        var typeDefinition = await _repo.GetByIdAsync(TypeDefinitionId.Of(cmd.TypeDefinitionId), ct);
        if (typeDefinition is null)
            throw new BusinessException($"TypeDefinition [{cmd.TypeDefinitionId}] not found.");

        // uniqueness on Libelle
        var duplicateLibelle = await _repo.GetOneByConditionAsync(t => t.Libelle == cmd.Libelle, ct);
        if (duplicateLibelle is not null && duplicateLibelle.Id != typeDefinition.Id)
            throw new TypeDefinitionLibelleAlreadyExistException(cmd.Libelle);

        typeDefinition.Update(cmd.Libelle, cmd.Description, cmd.IsEnabled);

        _repo.Update(typeDefinition);
        await _repo.SaveChangesAsync(ct);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, ct);

        return Result.Success(true);
    }
}