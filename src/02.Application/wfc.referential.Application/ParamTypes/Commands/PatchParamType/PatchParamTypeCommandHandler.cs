using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ParamTypeAggregate.Exceptions;

namespace wfc.referential.Application.ParamTypes.Commands.PatchParamType;

public class PatchParamTypeCommandHandler : ICommandHandler<PatchParamTypeCommand, Result<bool>>
{
    private readonly IParamTypeRepository _repo;
    private readonly ICacheService _cacheService;

    public PatchParamTypeCommandHandler(IParamTypeRepository repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(PatchParamTypeCommand cmd, CancellationToken ct)
    {
        var paramType = await _repo.GetByIdAsync(ParamTypeId.Of(cmd.ParamTypeId), ct);
        if (paramType is null)
            throw new ResourceNotFoundException($"ParamType [{cmd.ParamTypeId}] not found.");

        // duplicate Value check
        if (!string.IsNullOrWhiteSpace(cmd.Value))
        {
            var dup = await _repo.GetOneByConditionAsync(p => p.Value == cmd.Value, ct);
            if (dup is not null && dup.Id != paramType.Id)
                throw new ParamTypeValueAlreadyExistException(cmd.Value);
        }

        paramType.Patch(cmd.Value, cmd.IsEnabled);

        _repo.Update(paramType);
        await _repo.SaveChangesAsync(ct);

        await _cacheService.RemoveByPrefixAsync(CacheKeys.ParamType.Prefix, ct);

        return Result.Success(true);
    }
}