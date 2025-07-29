using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Commands.UpdateCurrencyDenomination;

public class UpdateCurrencyDenominationCommandHandler
    : ICommandHandler<UpdateCurrencyDenominationCommand, Result<bool>>
{
    private readonly ICurrencyDenominationRepository _repo;

    public UpdateCurrencyDenominationCommandHandler(ICurrencyDenominationRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(UpdateCurrencyDenominationCommand cmd, CancellationToken ct)
    {
        var currencyDenomination = await _repo.GetByIdAsync(CurrencyDenominationId.Of(cmd.CurrencyDenominationId), ct);
        if (currencyDenomination is null)
            throw new BusinessException($"CurrencyDenomination [{cmd.CurrencyDenominationId}] not found.");

        currencyDenomination.Update(
            CurrencyId.Of(cmd.CurrencyId),
            cmd.Type,
            cmd.Value,
            cmd.IsEnabled);

        _repo.Update(currencyDenomination);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}