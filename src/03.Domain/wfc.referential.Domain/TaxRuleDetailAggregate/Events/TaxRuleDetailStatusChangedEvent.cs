using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Domain.TaxAggrTaxRuleDetailsAggregateegate.Events;

public record TaxRuleDetailStatusChangedEvent : IDomainEvent
{
    public Guid TaxRuleDetailsId { get; }
    public bool IsEnabled { get; }
    public DateTime OccurredOn { get; init; } = DateTime.Now;

    public TaxRuleDetailStatusChangedEvent(TaxRuleDetailAggregate.TaxRuleDetail taxRuleDetails)
    {
        TaxRuleDetailsId = taxRuleDetails.Id!.Value;
        IsEnabled = taxRuleDetails.IsEnabled;
    }
}
