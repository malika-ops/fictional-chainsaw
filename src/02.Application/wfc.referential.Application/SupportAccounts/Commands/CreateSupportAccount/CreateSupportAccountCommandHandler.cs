using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Exceptions;

namespace wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;

public class CreateSupportAccountCommandHandler : ICommandHandler<CreateSupportAccountCommand, Result<Guid>>
{
    private readonly ISupportAccountRepository _supportAccountRepository;

    public CreateSupportAccountCommandHandler(ISupportAccountRepository supportAccountRepository)
    {
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<Result<Guid>> Handle(CreateSupportAccountCommand command, CancellationToken ct)
    {
        // Check uniqueness on Code
        var existingSupportAccountByCode = await _supportAccountRepository.GetOneByConditionAsync(sa => sa.Code == command.Code, ct);
        if (existingSupportAccountByCode is not null)
            throw new SupportAccountCodeAlreadyExistException(command.Code);

        // Check uniqueness on AccountingNumber
        var existingSupportAccountByAccountingNumber = await _supportAccountRepository.GetOneByConditionAsync(sa => sa.AccountingNumber == command.AccountingNumber, ct);
        if (existingSupportAccountByAccountingNumber is not null)
            throw new SupportAccountAccountingNumberAlreadyExistException(command.AccountingNumber);

        var supportAccount = SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            command.Code,
            command.Description,
            command.Threshold,
            command.Limit,
            command.AccountBalance,
            command.AccountingNumber
        );

        await _supportAccountRepository.AddAsync(supportAccount, ct);
        await _supportAccountRepository.SaveChangesAsync(ct);
        return Result.Success(supportAccount.Id!.Value);
    }
}