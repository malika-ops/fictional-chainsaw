using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;

namespace wfc.referential.Application.CurrencyDenominations.Commands.PatchCurrencyDenomination;

public class PatchCurrencyDenominationCommandHandler : ICommandHandler<PatchCurrencyDenominationCommand, Result<bool>>
{
    private readonly ICurrencyDenominationRepository _CurrencyDenominationRepository;
    private readonly ICurrencyRepository _CurrencyRepository;

    public PatchCurrencyDenominationCommandHandler(ICurrencyDenominationRepository CurrencyDenominationRepository, ICurrencyRepository CurrencyRepository)
    {
        _CurrencyDenominationRepository = CurrencyDenominationRepository;
        _CurrencyRepository = CurrencyRepository;
    }

    public async Task<Result<bool>> Handle(PatchCurrencyDenominationCommand cmd, CancellationToken ct)
    {
        //Check if the currency denomination exists
        var currencydenomination = await _CurrencyDenominationRepository.GetByIdAsync(CurrencyDenominationId.Of(cmd.CurrencyDenominationId), ct);
        if (currencydenomination is null)
            throw new ResourceNotFoundException($"CurrencyDenomination [{cmd.CurrencyDenominationId}] not found.");

        //Check if the currency exists
        if (cmd.CurrencyId.HasValue)
        {
            var currency = await _CurrencyRepository.GetByIdAsync(CurrencyId.Of(cmd.CurrencyId.Value)) ??
                throw new ResourceNotFoundException($"Currency with Id '{cmd.CurrencyId}' not found");
        }

        //Update the currency denomination
        currencydenomination.Patch(
            cmd.CurrencyId.HasValue ? CurrencyId.Of(cmd.CurrencyId.Value) : null,
            cmd.Type,
            cmd.Value,
            cmd.IsEnabled);

        await _CurrencyDenominationRepository.SaveChangesAsync(ct);

        return Result.Success(true);
    }
}
