using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxRuleDetailAggregate.Events;
public record TaxRuleDetailCreatedEvent(TaxRuleDetail TaxRuleDetails) : IDomainEvent;