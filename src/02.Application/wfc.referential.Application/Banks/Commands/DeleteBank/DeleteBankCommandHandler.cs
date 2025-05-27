using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;

namespace wfc.referential.Application.Banks.Commands.DeleteBank;

public class DeleteBankCommandHandler : ICommandHandler<DeleteBankCommand, Result<bool>>
{
    private readonly IBankRepository _repo;

    public DeleteBankCommandHandler(IBankRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteBankCommand cmd, CancellationToken ct)
    {
        var bank = await _repo.GetByIdAsync(BankId.Of(cmd.BankId), ct);
        if (bank is null)
            throw new BusinessException($"Bank [{cmd.BankId}] not found.");

        bank.Disable();
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}