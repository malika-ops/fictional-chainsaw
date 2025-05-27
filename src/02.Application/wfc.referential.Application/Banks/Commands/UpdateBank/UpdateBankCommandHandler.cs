using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.UpdateBank;

public class UpdateBankCommandHandler
    : ICommandHandler<UpdateBankCommand, Result<bool>>
{
    private readonly IBankRepository _repo;

    public UpdateBankCommandHandler(IBankRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateBankCommand cmd, CancellationToken ct)
    {
        var bank = await _repo.GetByIdAsync(BankId.Of(cmd.BankId), ct);
        if (bank is null)
            throw new BusinessException($"Bank [{cmd.BankId}] not found.");

        // uniqueness on Code
        var duplicateCode = await _repo.GetOneByConditionAsync(b => b.Code == cmd.Code, ct);
        if (duplicateCode is not null && duplicateCode.Id != bank.Id)
            throw new BankCodeAlreadyExistException(cmd.Code);

        bank.Update(
            cmd.Code,
            cmd.Name,
            cmd.Abbreviation,
            cmd.IsEnabled);

        _repo.Update(bank);
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}