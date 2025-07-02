using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Queries.GetTaxRuleDetailById;

public class GetTaxRuleDetailByIdQueryHandler : IQueryHandler<GetTaxRuleDetailByIdQuery, GetTaxRuleDetailsResponse>
{
    private readonly ITaxRuleDetailRepository _taxRuleDetailRepository;

    public GetTaxRuleDetailByIdQueryHandler(ITaxRuleDetailRepository taxRuleDetailRepository)
    {
        _taxRuleDetailRepository = taxRuleDetailRepository;
    }

    public async Task<GetTaxRuleDetailsResponse> Handle(GetTaxRuleDetailByIdQuery query, CancellationToken ct)
    {
        var id = TaxRuleDetailsId.Of(query.TaxRuleDetailId);
        var entity = await _taxRuleDetailRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"TaxRuleDetail with id '{query.TaxRuleDetailId}' not found.");

        return entity.Adapt<GetTaxRuleDetailsResponse>();
    }
} 