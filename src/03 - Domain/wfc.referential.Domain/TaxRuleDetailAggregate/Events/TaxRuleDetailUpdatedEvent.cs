using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.TaxAggrTaxRuleDetailsAggregateegate.Events;

public record TaxRuleDetailUpdatedEvent(TaxRuleDetail TaxRuleDetails) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.Now;
}
