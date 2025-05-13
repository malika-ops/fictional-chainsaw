using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate.Exceptions;

namespace wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;

public record DeleteSupportAccountCommandHandler : ICommandHandler<DeleteSupportAccountCommand, bool>
{
    private readonly ISupportAccountRepository _supportAccountRepository;

    public DeleteSupportAccountCommandHandler(ISupportAccountRepository supportAccountRepository)
    {
        _supportAccountRepository = supportAccountRepository;
    }

    public async Task<bool> Handle(DeleteSupportAccountCommand request, CancellationToken cancellationToken)
    {
        var supportAccount = await _supportAccountRepository.GetByIdAsync(SupportAccountId.Of(request.SupportAccountId), cancellationToken);

        if (supportAccount == null)
            throw new InvalidSupportAccountDeletingException("Support account not found");

        // Disable the support account instead of physically deleting it
        supportAccount.Disable();

        await _supportAccountRepository.UpdateSupportAccountAsync(supportAccount, cancellationToken);

        return true;
    }
}