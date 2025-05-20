using BuildingBlocks.Core.Exceptions;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Domain.CorridorAggregate.Exceptions;

public class CorridorHasRulesException : BusinessException
{
    public CorridorHasRulesException(IEnumerable<TaxRuleDetail> rules)
        : base($"Cannot delete the region because it has associated cities.[{string.Join(", ", rules.Select(c => $"{(c.Id is not null ? c.Id!.Value : null)}"))}]")
    {
    }

}
