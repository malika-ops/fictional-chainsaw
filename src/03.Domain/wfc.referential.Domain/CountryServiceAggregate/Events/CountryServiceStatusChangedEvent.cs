using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryServiceAggregate.Events;

public record CountryServiceStatusChangedEvent(CountryService CountryService) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;

}