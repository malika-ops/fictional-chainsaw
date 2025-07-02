using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Affiliates.Dtos;

namespace wfc.referential.Application.Affiliates.Queries.GetAffiliateById;

public record GetAffiliateByIdQuery : IQuery<GetAffiliatesResponse>
{
    public Guid AffiliateId { get; init; }
} 