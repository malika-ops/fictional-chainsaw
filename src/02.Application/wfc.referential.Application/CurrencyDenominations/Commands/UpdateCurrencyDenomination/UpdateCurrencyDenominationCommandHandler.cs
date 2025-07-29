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
    private readonly ICurrencyDenominationRepository _CurrencyDenominationRepository;
    private readonly ICurrencyRepository _CurrencyRepository;

    public UpdateCurrencyDenominationCommandHandler(ICurrencyDenominationRepository currencyDenominationRepository, ICurrencyRepository currencyRepository)
    {
        _CurrencyDenominationRepository = currencyDenominationRepository;
        _CurrencyRepository = currencyRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCurrencyDenominationCommand cmd, CancellationToken ct)
    {
        //Check if the currency denomination exists
        var currencyDenomination = await _CurrencyDenominationRepository.GetByIdAsync(CurrencyDenominationId.Of(cmd.CurrencyDenominationId), ct);
        if (currencyDenomination is null)
            throw new BusinessException($"CurrencyDenomination [{cmd.CurrencyDenominationId}] not found.");

        //Check if the currency exists
        var currency = await _CurrencyRepository.GetByIdAsync(CurrencyId.Of(cmd.CurrencyId)) ??
                throw new ResourceNotFoundException($"Currency with Id '{cmd.CurrencyId}' not found");

        //Update the currency denomination
        currencyDenomination.Update(
            CurrencyId.Of(cmd.CurrencyId),
            cmd.Type,
            cmd.Value,
            cmd.IsEnabled);

        _CurrencyDenominationRepository.Update(currencyDenomination);

        //Save changes to the repository
        await _CurrencyDenominationRepository.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}