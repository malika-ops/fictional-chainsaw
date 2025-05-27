using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.PatchBank;

public class PatchBankCommandHandler : ICommandHandler<PatchBankCommand, Result<bool>>
{
    private readonly IBankRepository _repo;

    public PatchBankCommandHandler(IBankRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(PatchBankCommand cmd, CancellationToken ct)
    {
        var bank = await _repo.GetByIdAsync(BankId.Of(cmd.BankId), ct);
        if (bank is null)
            throw new ResourceNotFoundException($"Bank [{cmd.BankId}] not found.");

        // duplicate Code check
        if (!string.IsNullOrWhiteSpace(cmd.Code))
        {
            var dup = await _repo.GetOneByConditionAsync(b => b.Code == cmd.Code, ct);
            if (dup is not null && dup.Id != bank.Id)
                throw new BankCodeAlreadyExistException(cmd.Code);
        }

        bank.Patch(
            cmd.Code,
            cmd.Name,
            cmd.Abbreviation,
            cmd.IsEnabled);

        _repo.Update(bank);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
