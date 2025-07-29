using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate;
using wfc.referential.Domain.CurrencyDenominationAggregate.Exceptions;

namespace wfc.referential.Application.CurrencyDenominations.Commands.CreateCurrencyDenomination;

public class CreateCurrencyDenominationCommandHandler : ICommandHandler<CreateCurrencyDenominationCommand, Result<Guid>>
{
    private readonly ICurrencyDenominationRepository _CurrencyDenominationRepository;
    private readonly ICurrencyRepository _CurrencyRepository;
    public CreateCurrencyDenominationCommandHandler(ICurrencyDenominationRepository CurrencyDenominationRepository, ICurrencyRepository currencyRepository)
    {
        _CurrencyDenominationRepository = CurrencyDenominationRepository;
        _CurrencyRepository = currencyRepository;
    }
        
    public async Task<Result<Guid>> Handle(CreateCurrencyDenominationCommand command, CancellationToken ct)
    {
        //Check if the currency exists
        var currency = await _CurrencyRepository.GetByIdAsync(CurrencyId.Of(command.CurrencyId), ct) ??
            throw new ResourceNotFoundException($"Currency with Id '{command.CurrencyId}' not found");


        //Check if the currency denomination already exists with the same type and value
        var existingCurrencyDenomination = await _CurrencyDenominationRepository.GetByConditionAsync(
            br => br.CurrencyId == CurrencyId.Of(command.CurrencyId) 
            && br.Type == command.Type
            && br.Value == command.Value
            && br.IsEnabled, ct);

        if (existingCurrencyDenomination.Any())
            throw new CurrencyDenominationAlreadyExistException(command.CurrencyId,command.Type,command.Value);

        // Create the new currency denomination
        var currencyDenomination = CurrencyDenomination.Create(
            CurrencyDenominationId.Of(Guid.NewGuid()),
            CurrencyId.Of(command.CurrencyId),
            command.Type,
            command.Value
            );

        // Add the currency denomination to the repository and save changes
        await _CurrencyDenominationRepository.AddAsync(currencyDenomination, ct);
        await _CurrencyDenominationRepository.SaveChangesAsync(ct);

        // Return the Id of the created currency denomination
        return Result.Success(currencyDenomination.Id!.Value);
    }
}