using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.CountryServiceAggregate.Events;

public record CountryServicePatchedEvent(CountryService CountryService) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;

}