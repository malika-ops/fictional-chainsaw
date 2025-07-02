using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.PartnerCountries.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Application.PartnerCountries.Queries.GetPartnerCountryById;

public class GetPartnerCountryByIdQueryHandler : IQueryHandler<GetPartnerCountryByIdQuery, PartnerCountryResponse>
{
    private readonly IPartnerCountryRepository _partnerCountryRepository;

    public GetPartnerCountryByIdQueryHandler(IPartnerCountryRepository partnerCountryRepository)
    {
        _partnerCountryRepository = partnerCountryRepository;
    }

    public async Task<PartnerCountryResponse> Handle(GetPartnerCountryByIdQuery query, CancellationToken ct)
    {
        var id = PartnerCountryId.Of(query.PartnerCountryId);
        var entity = await _partnerCountryRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"PartnerCountry with id '{query.PartnerCountryId}' not found.");

        return entity.Adapt<PartnerCountryResponse>();
    }
} 