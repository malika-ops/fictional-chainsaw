using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ControleAggregate.Exceptions;

namespace wfc.referential.Application.Controles.Commands.UpdateControle;

public class UpdateControleCommandHandler : ICommandHandler<UpdateControleCommand, Result<bool>>
{
    private readonly IControleRepository _controleRepo;

    public UpdateControleCommandHandler(IControleRepository controleRepo)
    {
        _controleRepo = controleRepo;
    }

    public async Task<Result<bool>> Handle(UpdateControleCommand cmd, CancellationToken ct)
    {
        var controleId = ControleId.Of(cmd.ControleId);

        var controle = await _controleRepo.GetByIdAsync(controleId, ct)
            ?? throw new ResourceNotFoundException($"Controle with id '{cmd.ControleId}' not found.");

        var duplicate = await _controleRepo.GetOneByConditionAsync(
            c => c.Code.ToLower() == cmd.Code.ToLower(), ct);

        if (duplicate is not null && duplicate.Id!.Value != cmd.ControleId)
            throw new DuplicateControleCodeException(cmd.Code);

        controle.Update(cmd.Code, cmd.Name, cmd.IsEnabled);

        await _controleRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}