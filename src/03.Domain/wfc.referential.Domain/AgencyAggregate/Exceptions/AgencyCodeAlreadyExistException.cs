using BuildingBlocks.Core.Exceptions;


namespace wfc.referential.Domain.AgencyAggregate.Exceptions;

public class AgencyCodeAlreadyExistException(string code)
    : ConflictException($"Agency with code {code} already exists.");
