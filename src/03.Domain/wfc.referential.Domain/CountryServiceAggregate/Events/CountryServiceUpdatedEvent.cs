using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryServiceAggregate.Events;

public record CountryServiceUpdatedEvent(CountryService CountryService) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;

}