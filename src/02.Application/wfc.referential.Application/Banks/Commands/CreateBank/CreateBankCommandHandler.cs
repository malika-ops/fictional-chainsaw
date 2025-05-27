using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.BankAggregate.Exceptions;

namespace wfc.referential.Application.Banks.Commands.CreateBank;

public class CreateBankCommandHandler : ICommandHandler<CreateBankCommand, Result<Guid>>
{
    private readonly IBankRepository _bankRepository;

    public CreateBankCommandHandler(IBankRepository bankRepository)
        => _bankRepository = bankRepository;

    public async Task<Result<Guid>> Handle(CreateBankCommand command, CancellationToken ct)
    {
        var existingBankByCode = await _bankRepository.GetOneByConditionAsync(b => b.Code == command.Code, ct);
        if (existingBankByCode is not null)
            throw new BankCodeAlreadyExistException(command.Code);

        var bank = Bank.Create(
            BankId.Of(Guid.NewGuid()),
            command.Code,
            command.Name,
            command.Abbreviation);

        await _bankRepository.AddAsync(bank, ct);
        await _bankRepository.SaveChangesAsync(ct);
        return Result.Success(bank.Id!.Value);
    }
}