using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate.Exceptions;

namespace wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;

public class DeleteTypeDefinitionCommandHandler : ICommandHandler<DeleteTypeDefinitionCommand, Result<bool>>
{
    private readonly ITypeDefinitionRepository _repo;
    private readonly ICacheService _cacheService;

    public DeleteTypeDefinitionCommandHandler(ITypeDefinitionRepository repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(DeleteTypeDefinitionCommand cmd, CancellationToken ct)
    {
        var typeDefinition = await _repo.GetByIdAsync(TypeDefinitionId.Of(cmd.TypeDefinitionId), ct);
        if (typeDefinition is null)
            throw new BusinessException($"TypeDefinition [{cmd.TypeDefinitionId}] not found.");

        if (typeDefinition.ParamTypes.Count > 0)
            throw new TypeDefinitionLinkedToParamTypeException(cmd.TypeDefinitionId);

        typeDefinition.Disable();
        await _repo.SaveChangesAsync(ct);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.TypeDefinition.Prefix, ct);

        return Result.Success(true);
    }
}