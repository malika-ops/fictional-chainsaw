using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.BankAggregate.Exceptions;

public class BankCodeAlreadyExistException(string code)
    : ConflictException($"Bank with code {code} already exists.");