using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ControleAggregate.Exceptions;


namespace wfc.referential.Application.Controles.Commands.CreateControle;

public class CreateControleCommandHandler : ICommandHandler<CreateControleCommand, Result<Guid>>
{
    private readonly IControleRepository _controleRepo;

    public CreateControleCommandHandler(IControleRepository controleRepo)
    {
        _controleRepo = controleRepo;
    }

    public async Task<Result<Guid>> Handle(CreateControleCommand req, CancellationToken ct)
    {
        var existing = await _controleRepo.GetOneByConditionAsync(c => c.Code.ToLower() == req.Code.ToLower(), ct);
        if (existing is not null)
            throw new DuplicateControleCodeException(req.Code);

        var id = ControleId.Of(Guid.NewGuid());
        var entity = Controle.Create(id, req.Code, req.Name);

        await _controleRepo.AddAsync(entity, ct);
        await _controleRepo.SaveChangesAsync(ct);

        return Result.Success(entity.Id!.Value);
    }
}