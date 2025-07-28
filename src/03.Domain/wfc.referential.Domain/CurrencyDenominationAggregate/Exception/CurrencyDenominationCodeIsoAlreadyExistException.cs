using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CurrencyDenominationAggregate.Exceptions;

public class CurrencyDenominationAlreadyExistException(Guid currencyid, CurrencyDenominationType type , decimal value)
    : ConflictException($"CurrencyDenomination with currencyid = {currencyid} and type = {type} and value = {value} already exists.");