using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Exceptions;

public class CurrencyCodeIsoAlreadyExistException(int codeIso)
    : ConflictException($"Currency with ISO code {codeIso} already exists.");