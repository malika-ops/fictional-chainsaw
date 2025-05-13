using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exception;

namespace wfc.referential.Application.Currencies.Commands.PatchCurrency;

public class PatchCurrencyCommandHandler : ICommandHandler<PatchCurrencyCommand, Result<Guid>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public PatchCurrencyCommandHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(PatchCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _currencyRepository.GetByIdAsync(CurrencyId.Of(request.CurrencyId), cancellationToken);

        if (currency == null)
            return Result.Failure<Guid>("Currency not found");

        // Check if the code is already used by another currency
        if (request.Code != null && request.Code != currency.Code)
        {
            var existingCurrency = await _currencyRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingCurrency != null && existingCurrency.Id.Value != request.CurrencyId)
            {
                throw new CodeAlreadyExistException(request.Code);
            }
        }

        // Check if the codeiso is already used by another currency
        if (request.CodeIso.HasValue && request.CodeIso.Value != currency.CodeIso)
        {
            var existingCurrencyByCodeIso = await _currencyRepository.GetByCodeIsoAsync(request.CodeIso.Value, cancellationToken);
            if (existingCurrencyByCodeIso != null && existingCurrencyByCodeIso.Id.Value != request.CurrencyId)
            {
                throw new CodeIsoAlreadyExistException(request.CodeIso.Value);
            }
        }

        // Update the currency with partial data
        string code = request.Code ?? currency.Code;
        string name = request.Name ?? currency.Name;
        string codeAR = request.CodeAR ?? currency.CodeAR;
        string codeEN = request.CodeEN ?? currency.CodeEN;
        int codeiso = request.CodeIso ?? currency.CodeIso;

        currency.Patch(code, codeAR, codeEN, name, codeiso);

        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value && !currency.IsEnabled)
            {
                currency.Activate();
            }
            else if (!request.IsEnabled.Value && currency.IsEnabled)
            {
                currency.Disable();
            }
        }

        await _currencyRepository.UpdateCurrencyAsync(currency, cancellationToken);

        return Result.Success(currency.Id.Value);
    }
}