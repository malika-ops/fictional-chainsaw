using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Commands.DeleteCurrencyDenomination;

public class DeleteCurrencyDenominationCommandHandler : ICommandHandler<DeleteCurrencyDenominationCommand, Result<bool>>
{
    private readonly ICurrencyDenominationRepository _repo;

    public DeleteCurrencyDenominationCommandHandler(ICurrencyDenominationRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(DeleteCurrencyDenominationCommand cmd, CancellationToken ct)
    {
        var currencyDenomination = await _repo.GetByIdAsync(CurrencyDenominationId.Of(cmd.CurrencyDenominationId), ct);
        if (currencyDenomination is null)
            throw new BusinessException($"CurrencyDenomination [{cmd.CurrencyDenominationId}] not found.");

        currencyDenomination.Disable();
        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}