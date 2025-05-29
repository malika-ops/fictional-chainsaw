using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Caching.Interface;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;

public record DeleteTaxRuleDetailCommand : ICommand<Result<bool>>
{
    public Guid TaxRuleDetailsId { get; init; }

}