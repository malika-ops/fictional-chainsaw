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
        IBankRepository bankRepository)
    {
        _partnerAccountRepository = partnerAccountRepository;
        _bankRepository = bankRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePartnerAccountCommand request, CancellationToken cancellationToken)
    {
        // Check if the account number already exists
        var existingAccountNumber = await _partnerAccountRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);
        if (existingAccountNumber is not null)
            throw new PartnerAccountAlreadyExistException(request.AccountNumber);

        // Check if the RIB already exists
        var existingRIB = await _partnerAccountRepository.GetByRIBAsync(request.RIB, cancellationToken);
        if (existingRIB is not null)
            throw new BusinessException($"Partner account with RIB {request.RIB} already exists.");

        // Check if the Bank exist
        var bank = await _bankRepository.GetByIdAsync(new BankId(request.BankId), cancellationToken);
        if (bank is null)
            throw new BusinessException($"Bank with ID {request.BankId} not found");

        var id = PartnerAccountId.Of(Guid.NewGuid());
        var partnerAccount = PartnerAccount.Create(
            id,
            request.AccountNumber,
            request.RIB,
            request.Domiciliation,
            request.BusinessName,
            request.ShortName,
            request.AccountBalance,
            bank,
            request.AccountType
        );

        await _partnerAccountRepository.AddPartnerAccountAsync(partnerAccount, cancellationToken);

        return Result.Success(partnerAccount.Id.Value);
    }
}