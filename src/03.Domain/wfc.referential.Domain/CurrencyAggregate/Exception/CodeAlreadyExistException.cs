using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyAggregate.Exceptions;

public class CurrencyCodeAlreadyExistException(string code)
    : ConflictException($"Currency with code {code} already exists.");