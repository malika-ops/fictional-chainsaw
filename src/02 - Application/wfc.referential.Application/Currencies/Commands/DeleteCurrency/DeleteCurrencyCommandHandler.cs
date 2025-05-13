using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;

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
            return Result.Failure<bool>("Currency not found");


        currency.Disable();
        await _currencyRepository.UpdateCurrencyAsync(currency, cancellationToken);

        return Result.Success(true);
    }
}