using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Queries.GetPartnerById;

public record GetPartnerByIdQuery : IQuery<GetPartnersResponse>
{
    public Guid PartnerId { get; init; }
} 