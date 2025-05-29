using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SupportAccountAggregate.Exceptions;

public class SupportAccountAccountingNumberAlreadyExistException(string accountingNumber)
    : ConflictException($"Support account with accounting number {accountingNumber} already exists.");