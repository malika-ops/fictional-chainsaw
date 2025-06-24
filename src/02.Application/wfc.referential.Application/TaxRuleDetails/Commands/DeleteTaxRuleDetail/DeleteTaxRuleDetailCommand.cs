using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;

public record DeleteTaxRuleDetailCommand : ICommand<Result<bool>>
{
    public Guid TaxRuleDetailId { get; init; }

}