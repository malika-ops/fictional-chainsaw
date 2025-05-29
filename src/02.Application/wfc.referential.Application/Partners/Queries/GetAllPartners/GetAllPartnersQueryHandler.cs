using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Queries.GetAllPartners;

public class GetAllPartnersQueryHandler : IQueryHandler<GetAllPartnersQuery, PagedResult<GetPartnersResponse>>
{
    private readonly IPartnerRepository _repo;

    public GetAllPartnersQueryHandler(IPartnerRepository repo) => _repo = repo;

    public async Task<PagedResult<GetPartnersResponse>> Handle(
        GetAllPartnersQuery partnerQuery, CancellationToken ct)
    {
        var partners = await _repo.GetPagedByCriteriaAsync(partnerQuery, partnerQuery.PageNumber, partnerQuery.PageSize, ct);
        return new PagedResult<GetPartnersResponse>(partners.Items.Adapt<List<GetPartnersResponse>>(), partners.TotalCount, partners.PageNumber, partners.PageSize);
    }
}