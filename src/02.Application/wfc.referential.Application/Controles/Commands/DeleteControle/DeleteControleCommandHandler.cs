using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;

namespace wfc.referential.Application.Controles.Commands.DeleteControle;

public class DeleteControleCommandHandler : ICommandHandler<DeleteControleCommand, Result<bool>>
{
    private readonly IControleRepository _controleRepo;

    public DeleteControleCommandHandler(IControleRepository controleRepo)
    {
        _controleRepo = controleRepo;
    }

    public async Task<Result<bool>> Handle(DeleteControleCommand req, CancellationToken ct)
    {
        var controleId = ControleId.Of(req.ControleId);

        var entity = await _controleRepo.GetByIdAsync(controleId, ct)
            ?? throw new ResourceNotFoundException($"Controle {req.ControleId} not found.");

        entity.Disable();

        await _controleRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}