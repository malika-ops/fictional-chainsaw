using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyAggregate.Exceptions;

namespace wfc.referential.Application.Currencies.Commands.CreateCurrency;

public class CreateCurrencyCommandHandler : ICommandHandler<CreateCurrencyCommand, Result<Guid>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public CreateCurrencyCommandHandler(ICurrencyRepository currencyRepository)
        => _currencyRepository = currencyRepository;

    public async Task<Result<Guid>> Handle(CreateCurrencyCommand command, CancellationToken ct)
    {
        var existingCurrencyByCode = await _currencyRepository.GetOneByConditionAsync(c => c.Code == command.Code, ct);
        if (existingCurrencyByCode is not null)
            throw new CurrencyCodeAlreadyExistException(command.Code);

        var existingCurrencyByCodeIso = await _currencyRepository.GetOneByConditionAsync(c => c.CodeIso == command.CodeIso, ct);
        if (existingCurrencyByCodeIso is not null)
            throw new CurrencyCodeIsoAlreadyExistException(command.CodeIso);

        var currency = Currency.Create(
            CurrencyId.Of(Guid.NewGuid()),
            command.Code,
            command.CodeAR,
            command.CodeEN,
            command.Name,
            command.CodeIso);

        await _currencyRepository.AddAsync(currency, ct);
        await _currencyRepository.SaveChangesAsync(ct);
        return Result.Success(currency.Id!.Value);
    }
}