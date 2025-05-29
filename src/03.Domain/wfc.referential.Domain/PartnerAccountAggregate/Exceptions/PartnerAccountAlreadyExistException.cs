using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

public class PartnerAccountAlreadyExistException(string accountNumber)
    : ConflictException($"Partner account with account number {accountNumber} already exists.");