using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Exceptions;

namespace wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;

public record CreateSupportAccountCommandHandler : ICommandHandler<CreateSupportAccountCommand, Result<Guid>>
{
    private readonly ISupportAccountRepository _supportAccountRepository;
    private readonly IPartnerRepository _partnerRepository;

    public CreateSupportAccountCommandHandler(
        ISupportAccountRepository supportAccountRepository,
        IPartnerRepository partnerRepository)
    {
        _supportAccountRepository = supportAccountRepository;
        _partnerRepository = partnerRepository;
    }

    public async Task<Result<Guid>> Handle(CreateSupportAccountCommand request, CancellationToken cancellationToken)
    {
        // Check if the code already exists
        var existingCode = await _supportAccountRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode is not null)
            throw new SupportAccountAlreadyExistException(request.Code);

        // Check if the accounting number already exists
        var existingAccountingNumber = await _supportAccountRepository.GetByAccountingNumberAsync(request.AccountingNumber, cancellationToken);
        if (existingAccountingNumber is not null)
            throw new BusinessException($"Support account with accounting number {request.AccountingNumber} already exists.");

        // Check if the Partner exists
        var partner = await _partnerRepository.GetByIdAsync(new PartnerId(request.PartnerId), cancellationToken);
        if (partner is null)
            throw new BusinessException($"Partner with ID {request.PartnerId} not found");

        var id = SupportAccountId.Of(Guid.NewGuid());
        var supportAccount = SupportAccount.Create(
            id,
            request.Code,
            request.Name,
            request.Threshold,
            request.Limit,
            request.AccountBalance,
            request.AccountingNumber,
            partner,
            request.SupportAccountType
        );

        await _supportAccountRepository.AddSupportAccountAsync(supportAccount, cancellationToken);

        return Result.Success(supportAccount.Id.Value);
    }
}