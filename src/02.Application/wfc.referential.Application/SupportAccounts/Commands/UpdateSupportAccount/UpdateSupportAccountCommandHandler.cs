using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Exceptions;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;

public record UpdateSupportAccountCommandHandler : ICommandHandler<UpdateSupportAccountCommand, Guid>
{
    private readonly ISupportAccountRepository _supportAccountRepository;
    private readonly IPartnerRepository _partnerRepository;

    public UpdateSupportAccountCommandHandler(
        ISupportAccountRepository supportAccountRepository,
        IPartnerRepository partnerRepository)
    {
        _supportAccountRepository = supportAccountRepository;
        _partnerRepository = partnerRepository;
    }

    public async Task<Guid> Handle(UpdateSupportAccountCommand request, CancellationToken cancellationToken)
    {
        // Check if support account exists
        var supportAccount = await _supportAccountRepository.GetByIdAsync(new SupportAccountId(request.SupportAccountId), cancellationToken);
        if (supportAccount is null)
            throw new BusinessException($"Support account with ID {request.SupportAccountId} not found");

        // Check if code is unique (if changed)
        if (request.Code != supportAccount.Code)
        {
            var existingWithCode = await _supportAccountRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingWithCode is not null && existingWithCode.Id.Value != request.SupportAccountId)
                throw new SupportAccountAlreadyExistException(request.Code);
        }

        // Check if accounting number is unique (if changed)
        if (request.AccountingNumber != supportAccount.AccountingNumber)
        {
            var existingWithAccountingNumber = await _supportAccountRepository.GetByAccountingNumberAsync(request.AccountingNumber, cancellationToken);
            if (existingWithAccountingNumber is not null && existingWithAccountingNumber.Id.Value != request.SupportAccountId)
                throw new BusinessException($"Support account with accounting number {request.AccountingNumber} already exists.");
        }

        // Get the partner
        var partner = await _partnerRepository.GetByIdAsync(new PartnerId(request.PartnerId), cancellationToken);
        if (partner is null)
            throw new BusinessException($"Partner with ID {request.PartnerId} not found");

        // Update the support account
        supportAccount.Update(
            request.Code,
            request.Name,
            request.Threshold,
            request.Limit,
            request.AccountBalance,
            request.AccountingNumber,
            partner,
            request.SupportAccountType
        );

        // Handle enabled status changes through the proper domain methods
        if (request.IsEnabled && !supportAccount.IsEnabled)
        {
            supportAccount.Activate();
        }
        else if (!request.IsEnabled && supportAccount.IsEnabled)
        {
            supportAccount.Disable();
        }

        await _supportAccountRepository.UpdateSupportAccountAsync(supportAccount, cancellationToken);

        return supportAccount.Id.Value;
    }
}