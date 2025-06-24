using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Queries.GetFiltredPartners;

public class GetFiltredPartnersQueryHandler : IQueryHandler<GetFiltredPartnersQuery, PagedResult<GetPartnersResponse>>
{
    private readonly IPartnerRepository _repo;

    public GetFiltredPartnersQueryHandler(IPartnerRepository repo) => _repo = repo;

    public async Task<PagedResult<GetPartnersResponse>> Handle(
        GetFiltredPartnersQuery partnerQuery, CancellationToken ct)
    {
        var partners = await _repo.GetPagedByCriteriaAsync(partnerQuery, partnerQuery.PageNumber, partnerQuery.PageSize, ct);
        return new PagedResult<GetPartnersResponse>(partners.Items.Adapt<List<GetPartnersResponse>>(), partners.TotalCount, partners.PageNumber, partners.PageSize);
    }
}