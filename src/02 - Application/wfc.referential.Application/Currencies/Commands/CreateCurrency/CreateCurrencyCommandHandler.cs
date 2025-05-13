using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exception;

namespace wfc.referential.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyCommandHandler : ICommandHandler<CreateCurrencyCommand, Result<Guid>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public CreateCurrencyCommandHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCurrencyCommand command, CancellationToken cancellationToken)
    {
        // Check if currency with same code already exists
        var existingCurrencyByCode = await _currencyRepository.GetByCodeAsync(command.Code, cancellationToken);

        if (existingCurrencyByCode != null)
        {
            throw new CodeAlreadyExistException(command.Code);
        }

        // Check if currency with same codeiso already exists
        var existingCurrencyByCodeIso = await _currencyRepository.GetByCodeIsoAsync(command.CodeIso, cancellationToken);

        if (existingCurrencyByCodeIso != null)
        {
            throw new CodeIsoAlreadyExistException(command.CodeIso);
        }

        // Create new currency
        var currency = Currency.Create(
            CurrencyId.Of(Guid.NewGuid()),
            command.Code,
            command.CodeAR,
            command.CodeEN,
            command.Name,
            command.CodeIso
        );

        // Save to repository
        await _currencyRepository.AddCurrencyAsync(currency, cancellationToken);

        // Return ID of new currency
        return Result.Success(currency.Id.Value);
    }
}