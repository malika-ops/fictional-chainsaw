using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerAggregate;

namespace wfc.referential.Application.Partners.Queries.GetPartnerById;

public class GetPartnerByIdQueryHandler : IQueryHandler<GetPartnerByIdQuery, GetPartnersResponse>
{
    private readonly IPartnerRepository _partnerRepository;

    public GetPartnerByIdQueryHandler(IPartnerRepository partnerRepository)
    {
        _partnerRepository = partnerRepository;
    }

    public async Task<GetPartnersResponse> Handle(GetPartnerByIdQuery query, CancellationToken ct)
    {
        var id = PartnerId.Of(query.PartnerId);
        var entity = await _partnerRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"Partner with id '{query.PartnerId}' not found.");

        return entity.Adapt<GetPartnersResponse>();
    }
} 