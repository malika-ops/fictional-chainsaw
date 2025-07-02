using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Affiliates.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;

namespace wfc.referential.Application.Affiliates.Queries.GetAffiliateById;

public class GetAffiliateByIdQueryHandler : IQueryHandler<GetAffiliateByIdQuery, GetAffiliatesResponse>
{
    private readonly IAffiliateRepository _affiliateRepository;

    public GetAffiliateByIdQueryHandler(IAffiliateRepository affiliateRepository)
    {
        _affiliateRepository = affiliateRepository;
    }

    public async Task<GetAffiliatesResponse> Handle(GetAffiliateByIdQuery query, CancellationToken ct)
    {
        var id = AffiliateId.Of(query.AffiliateId);
        var entity = await _affiliateRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Affiliate with id '{query.AffiliateId}' not found.");

        return entity.Adapt<GetAffiliatesResponse>();
    }
} 