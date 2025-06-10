using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryServiceAggregate.Events;


public record CountryServiceCreatedEvent(CountryService CountryService) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;

}
