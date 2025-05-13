using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;

public record UpdateSupportAccountBalanceCommandHandler : ICommandHandler<UpdateSupportAccountBalanceCommand, Guid>
{
    private readonly ISupportAccountRepository _supportAccountRepository;

    public UpdateSupportAccountBalanceCommandHandler(ISupportAccountRepository supportAccountRepository)
    {
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<Guid> Handle(UpdateSupportAccountBalanceCommand request, CancellationToken cancellationToken)
    {
        // Check if support account exists
        var supportAccount = await _supportAccountRepository.GetByIdAsync(new SupportAccountId(request.SupportAccountId), cancellationToken);
        if (supportAccount is null)
            throw new BusinessException($"Support account with ID {request.SupportAccountId} not found");

        // Update the balance using dedicated domain method
        supportAccount.UpdateBalance(request.NewBalance);

        await _supportAccountRepository.UpdateSupportAccountAsync(supportAccount, cancellationToken);

        return supportAccount.Id.Value;
    }
}