using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public class PatchTypeDefinitionCommandHandler : ICommandHandler<PatchTypeDefinitionCommand, Result<bool>>
{
    private readonly ITypeDefinitionRepository _repo;
    private readonly ICacheService _cacheService;

    public PatchTypeDefinitionCommandHandler(ITypeDefinitionRepository repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(PatchTypeDefinitionCommand cmd, CancellationToken ct)
    {
        var typeDefinition = await _repo.GetByIdAsync(TypeDefinitionId.Of(cmd.TypeDefinitionId), ct);
        if (typeDefinition is null)
            throw new ResourceNotFoundException($"TypeDefinition [{cmd.TypeDefinitionId}] not found.");

        // duplicate Libelle check
        if (!string.IsNullOrWhiteSpace(cmd.Libelle))
        {
            var dup = await _repo.GetOneByConditionAsync(t => t.Libelle == cmd.Libelle, ct);
            if (dup is not null && dup.Id != typeDefinition.Id)
                throw new TypeDefinitionLibelleAlreadyExistException(cmd.Libelle);
        }

        typeDefinition.Patch(cmd.Libelle, cmd.Description, cmd.IsEnabled);

        _repo.Update(typeDefinition);
        await _repo.SaveChangesAsync(ct);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, ct);

        return Result.Success(true);
    }
}