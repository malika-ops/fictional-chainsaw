using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exception;

namespace wfc.referential.Application.Currencies.Commands.PatchCurrency;

public class PatchCurrencyCommandHandler : ICommandHandler<PatchCurrencyCommand, Result<Guid>>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ICacheService _cache;

    public PatchCurrencyCommandHandler(ICurrencyRepository currencyRepository, ICacheService cache)
    {
        _currencyRepository = currencyRepository;
        _cache = cache;
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
            if (existingCurrencyByCodeIso != null && existingCurrencyByCodeIso.Id!.Value != request.CurrencyId)
            {
                throw new CodeIsoAlreadyExistException(request.CodeIso.Value);
            }
        }

        // Patch the currency with partial data

        currency.Patch(request.Code, request.CodeAR, request.CodeEN, request.Name, request.CodeIso , request.IsEnabled);

     
        await _currencyRepository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByPrefixAsync("ReferentialCache:currencies_", cancellationToken);

        return Result.Success(currency.Id!.Value);
    }
}