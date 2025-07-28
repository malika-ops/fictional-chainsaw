using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Commands.PatchCurrencyDenomination;

public class PatchCurrencyDenominationCommandHandler : ICommandHandler<PatchCurrencyDenominationCommand, Result<bool>>
{
    private readonly ICurrencyDenominationRepository _repo;

    public PatchCurrencyDenominationCommandHandler(ICurrencyDenominationRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(PatchCurrencyDenominationCommand cmd, CancellationToken ct)
    {
        var currencydenomination = await _repo.GetByIdAsync(CurrencyDenominationId.Of(cmd.CurrencyDenominationId), ct);
        if (currencydenomination is null)
            throw new ResourceNotFoundException($"CurrencyDenomination [{cmd.CurrencyDenominationId}] not found.");

        currencydenomination.Patch(
            cmd.CurrencyId.HasValue ? CurrencyId.Of(cmd.CurrencyId.Value) : null,
            cmd.Type,
            cmd.Value,
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
