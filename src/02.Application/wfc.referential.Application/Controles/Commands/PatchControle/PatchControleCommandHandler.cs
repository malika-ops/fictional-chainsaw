using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ControleAggregate.Exceptions;

namespace wfc.referential.Application.Controles.Commands.PatchControle;

public record PatchControleCommandHandler : ICommandHandler<PatchControleCommand, Result<bool>>
{
    private readonly IControleRepository _controleRepo;

    public PatchControleCommandHandler(IControleRepository controleRepo)
    {
        _controleRepo = controleRepo;
    }

    public async Task<Result<bool>> Handle(PatchControleCommand cmd, CancellationToken ct)
    {
        var controleId = ControleId.Of(cmd.ControleId);

        var controle = await _controleRepo.GetByIdAsync(controleId, ct)
            ?? throw new ResourceNotFoundException($"Controle with id '{cmd.ControleId}' not found.");

        if (cmd.Code is not null)
        {
            var existing = await _controleRepo.GetOneByConditionAsync(
                c => c.Code.ToLower() == cmd.Code.ToLower(), ct);

            if (existing is not null && existing.Id!.Value != cmd.ControleId)
                throw new DuplicateControleCodeException(cmd.Code);
        }

        controle.Patch(cmd.Code, cmd.Name, cmd.IsEnabled);

        await _controleRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
