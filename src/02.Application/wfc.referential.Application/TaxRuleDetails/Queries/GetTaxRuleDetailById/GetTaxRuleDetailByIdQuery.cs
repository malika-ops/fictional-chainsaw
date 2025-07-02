using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.TaxRuleDetails.Dtos;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetTaxRuleDetailById;

public record GetTaxRuleDetailByIdQuery : IQuery<GetTaxRuleDetailsResponse>
{
    public Guid TaxRuleDetailId { get; init; }
} 