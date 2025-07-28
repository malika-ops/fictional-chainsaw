using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate.Exceptions;

namespace wfc.referential.Application.CurrencyDenominations.Commands.CreateCurrencyDenomination;

public class CreateCurrencyDenominationCommandHandler : ICommandHandler<CreateCurrencyDenominationCommand, Result<Guid>>
{
    private readonly ICurrencyDenominationRepository _CurrencyDenominationRepository;

    public CreateCurrencyDenominationCommandHandler(ICurrencyDenominationRepository CurrencyDenominationRepository)
        => _CurrencyDenominationRepository = CurrencyDenominationRepository;

    public async Task<Result<Guid>> Handle(CreateCurrencyDenominationCommand command, CancellationToken ct)
    {
        var existingCurrencyDenomination = await _CurrencyDenominationRepository.GetByConditionAsync(
            br => br.CurrencyId == CurrencyId.Of(command.CurrencyId) 
            && br.Type == command.Type
            && br.Value == command.Value
            && br.IsEnabled, ct);

        if (existingCurrencyDenomination is not null)
            throw new CurrencyDenominationAlreadyExistException(command.CurrencyId,command.Type,command.Value);
        

        var currencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(Guid.NewGuid()),
            CurrencyId.Of(command.CurrencyId),
            command.Type,
            command.Value
            );

        await _CurrencyDenominationRepository.AddAsync(currencyDenomination, ct);
        await _CurrencyDenominationRepository.SaveChangesAsync(ct);
        return Result.Success(currencyDenomination.Id!.Value);
    }
}