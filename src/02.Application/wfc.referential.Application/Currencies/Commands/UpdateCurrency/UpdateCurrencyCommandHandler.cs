using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exception;

namespace wfc.referential.Application.Currencies.Commands.UpdateCurrency;

public class UpdateCurrencyCommandHandler : ICommandHandler<UpdateCurrencyCommand, Result<Guid>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public UpdateCurrencyCommandHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _currencyRepository.GetByIdAsync(new CurrencyId(request.CurrencyId), cancellationToken);

        if (currency == null)
            return Result.Failure<Guid>("Currency not found");

        // Check if the code is already used by another currency
        if (currency.Code != request.Code)
        {
            var existingCurrency = await _currencyRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingCurrency != null && existingCurrency.Id.Value != request.CurrencyId)
            {
                throw new CodeAlreadyExistException(request.Code);
            }
        }

        // Check if the codeiso is already used by another currency
        if (currency.CodeIso != request.CodeIso)
        {
            var existingCurrencyByCodeIso = await _currencyRepository.GetByCodeIsoAsync(request.CodeIso, cancellationToken);
            if (existingCurrencyByCodeIso != null && existingCurrencyByCodeIso.Id.Value != request.CurrencyId)
            {
                throw new CodeIsoAlreadyExistException(request.CodeIso);
            }
        }

        currency.Update(
            request.Code,
            request.CodeAR,
            request.CodeEN,
            request.Name,
            request.CodeIso
        );

        if (request.IsEnabled && !currency.IsEnabled)
        {
            currency.Activate();
        }
        else if (!request.IsEnabled && currency.IsEnabled)
        {
            currency.Disable();
        }

        await _currencyRepository.UpdateCurrencyAsync(currency, cancellationToken);

        return Result.Success(currency.Id.Value);
    }
}