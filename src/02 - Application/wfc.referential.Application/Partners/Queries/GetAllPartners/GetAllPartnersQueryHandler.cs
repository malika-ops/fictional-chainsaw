using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Queries.GetAllPartners;

public record GetAllPartnersQueryHandler : IQueryHandler<GetAllPartnersQuery, PagedResult<PartnerResponse>>
{
    private readonly IPartnerRepository _partnerRepository;

    public GetAllPartnersQueryHandler(IPartnerRepository partnerRepository)
    {
        _partnerRepository = partnerRepository;
    }

    public async Task<PagedResult<PartnerResponse>> Handle(GetAllPartnersQuery request, CancellationToken cancellationToken)
    {
        var partners = await _partnerRepository
            .GetFilteredPartnersAsync(request, cancellationToken);

        int totalCount = await _partnerRepository
            .GetCountTotalAsync(request, cancellationToken);

        var partnerResponses = partners.Adapt<List<PartnerResponse>>();

        return new PagedResult<PartnerResponse>(partnerResponses, totalCount, request.PageNumber, request.PageSize);
    }
}