using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.OperatorAggregate.Events;

public record OperatorCreatedEvent(
    Guid OperatorId,
    string Code,
    string IdentityCode,
    string LastName,
    string FirstName,
    string Email,
    string PhoneNumber,
    DateTime OccurredOn) : IDomainEvent;