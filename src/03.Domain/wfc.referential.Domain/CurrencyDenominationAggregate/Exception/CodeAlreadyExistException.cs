using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Exceptions;

public class CurrencyDenominationCodeAlreadyExistException(string code)
    : ConflictException($"CurrencyDenomination with code {code} already exists.");