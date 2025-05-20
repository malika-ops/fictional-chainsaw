using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exception;

namespace wfc.referential.Application.Currencies.Commands.DeleteCurrency;

public class DeleteCurrencyCommandHandler : ICommandHandler<DeleteCurrencyCommand, Result<bool>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public DeleteCurrencyCommandHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<bool>> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currencyId = Guid.Parse(request.CurrencyId);
        var currency = await _currencyRepository.GetByIdAsync(CurrencyId.Of(currencyId), cancellationToken);

        if (currency == null)
            throw new BusinessException("Currency not found");

        // Check if currency is associated with any countries before allowing deletion
        bool isAssociatedWithCountry = await _currencyRepository.IsCurrencyAssociatedWithCountryAsync(currency.Id, cancellationToken);
        if (isAssociatedWithCountry)
            throw new BusinessException("Cannot delete currency that is associated with countries");

        currency.Disable();
        await _currencyRepository.UpdateCurrencyAsync(currency, cancellationToken);

        return Result.Success(true);
    }
}