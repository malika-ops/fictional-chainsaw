using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ServiceControleAggregate;

namespace wfc.referential.Application.ServiceControles.Commands.DeleteServiceControle;

public class DeleteServiceControleCommandHandler : ICommandHandler<DeleteServiceControleCommand, Result<bool>>
{
    private readonly IServiceControleRepository _ServiceControleRepo;

    public DeleteServiceControleCommandHandler(IServiceControleRepository serviceControleRepo)
    {
        _ServiceControleRepo = serviceControleRepo;
    }

    public async Task<Result<bool>> Handle(DeleteServiceControleCommand req, CancellationToken ct)
    {
        var id = ServiceControleId.Of(req.ServiceControleId);
        var entity = await _ServiceControleRepo.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"ServiceControle {req.ServiceControleId} not found.");

        entity.Disable();          

        await _ServiceControleRepo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}