using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Exceptions;

namespace wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;

public record PatchSupportAccountCommandHandler : ICommandHandler<PatchSupportAccountCommand, Guid>
{
    private readonly ISupportAccountRepository _supportAccountRepository;
    private readonly IPartnerRepository _partnerRepository;

    public PatchSupportAccountCommandHandler(
        ISupportAccountRepository supportAccountRepository,
        IPartnerRepository partnerRepository)
    {
        _supportAccountRepository = supportAccountRepository;
        _partnerRepository = partnerRepository;
    }

    public async Task<Guid> Handle(PatchSupportAccountCommand request, CancellationToken cancellationToken)
    {
        var supportAccount = await _supportAccountRepository.GetByIdAsync(new SupportAccountId(request.SupportAccountId), cancellationToken);
        if (supportAccount == null)
            throw new BusinessException("Support account not found");

        // Check if code is unique if it's being updated
        if (request.Code != null && request.Code != supportAccount.Code)
        {
            var existingWithCode = await _supportAccountRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingWithCode != null && existingWithCode.Id.Value != request.SupportAccountId)
                throw new SupportAccountAlreadyExistException(request.Code);
        }

        // Check if accounting number is unique if it's being updated
        if (request.AccountingNumber != null && request.AccountingNumber != supportAccount.AccountingNumber)
        {
            var existingWithAccountingNumber = await _supportAccountRepository.GetByAccountingNumberAsync(request.AccountingNumber, cancellationToken);
            if (existingWithAccountingNumber != null && existingWithAccountingNumber.Id.Value != request.SupportAccountId)
                throw new BusinessException($"Support account with accounting number {request.AccountingNumber} already exists.");
        }

        // Get updated partner if needed
        var partner = supportAccount.Partner;
        if (request.PartnerId.HasValue && request.PartnerId.Value != supportAccount.Partner.Id.Value)
        {
            var updatedPartner = await _partnerRepository.GetByIdAsync(new PartnerId(request.PartnerId.Value), cancellationToken);
            if (updatedPartner == null)
                throw new BusinessException($"Partner with ID {request.PartnerId} not found");
            partner = updatedPartner;
        }

        // Collect updates for domain entities
        var updatedCode = request.Code ?? supportAccount.Code;
        var updatedName = request.Name ?? supportAccount.Name;
        var updatedThreshold = request.Threshold ?? supportAccount.Threshold;
        var updatedLimit = request.Limit ?? supportAccount.Limit;
        var updatedAccountBalance = request.AccountBalance ?? supportAccount.AccountBalance;
        var updatedAccountingNumber = request.AccountingNumber ?? supportAccount.AccountingNumber;
        var updatedSupportAccountType = request.SupportAccountType ?? supportAccount.SupportAccountType;

        // Update via domain methods
        supportAccount.Patch(
            updatedCode,
            updatedName,
            updatedThreshold,
            updatedLimit,
            updatedAccountBalance,
            updatedAccountingNumber,
            partner,
            updatedSupportAccountType
        );

        // Handle IsEnabled status changes separately through the proper domain methods
        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !supportAccount.IsEnabled)
            {
                supportAccount.Activate();
            }
            else if (!request.IsEnabled.Value && supportAccount.IsEnabled)
            {
                supportAccount.Disable();
            }
        }

        await _supportAccountRepository.UpdateSupportAccountAsync(supportAccount, cancellationToken);

        return supportAccount.Id.Value;
    }
}