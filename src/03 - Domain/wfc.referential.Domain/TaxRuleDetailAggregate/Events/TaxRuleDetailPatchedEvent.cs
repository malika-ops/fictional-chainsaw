using BuildingBlocks.Core.Abstraction.Domain;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.TaxRuleDetailAggregate.Events;

public record TaxRuleDetailPatchedEvent(TaxRuleDetail TaxRuleDetails) : IDomainEvent;