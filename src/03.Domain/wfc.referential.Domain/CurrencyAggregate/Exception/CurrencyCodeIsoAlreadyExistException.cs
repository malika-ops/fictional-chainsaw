using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyAggregate.Exceptions;

public class CurrencyCodeIsoAlreadyExistException(int codeIso)
    : ConflictException($"Currency with ISO code {codeIso} already exists.");