using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

namespace wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;

public class CreatePartnerAccountCommandHandler : ICommandHandler<CreatePartnerAccountCommand, Result<Guid>>
{
    private readonly IPartnerAccountRepository _partnerAccountRepository;
    private readonly IBankRepository _bankRepository;

    public CreatePartnerAccountCommandHandler(
        IPartnerAccountRepository partnerAccountRepository,
        IBankRepository bankRepository,
        IParamTypeRepository paramTypeRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
        _bankRepository = bankRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerAccountCommand command, CancellationToken ct)
    {
        // Check uniqueness on AccountNumber
        var existingAccountNumber = await _partnerAccountRepository.GetOneByConditionAsync(p => p.AccountNumber == command.AccountNumber, ct);
        if (existingAccountNumber is not null)
            throw new PartnerAccountAlreadyExistException(command.AccountNumber);

        // Check uniqueness on RIB  
        var existingRIB = await _partnerAccountRepository.GetOneByConditionAsync(p => p.RIB == command.RIB, ct);
        if (existingRIB is not null)
            throw new PartnerAccountAlreadyExistException(command.RIB);

        // Validate Bank exists
        var bank = await _bankRepository.GetByIdAsync(BankId.Of(command.BankId), ct);
        if (bank is null)
            throw new BusinessException($"Bank with ID {command.BankId} not found");

        var partnerAccount = PartnerAccount.Create(
            PartnerAccountId.Of(Guid.NewGuid()),
            command.AccountNumber,
            command.RIB,
            command.Domiciliation,
            command.BusinessName,
            command.ShortName,
            command.AccountBalance,
            bank,
            command.PartnerAccountType);

        await _partnerAccountRepository.AddAsync(partnerAccount, ct);
        await _partnerAccountRepository.SaveChangesAsync(ct);

        return Result.Success(partnerAccount.Id!.Value);
    }
}