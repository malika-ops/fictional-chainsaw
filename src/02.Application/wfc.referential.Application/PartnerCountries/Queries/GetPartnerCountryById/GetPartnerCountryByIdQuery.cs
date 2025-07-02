using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.PartnerCountries.Dtos;

namespace wfc.referential.Application.PartnerCountries.Queries.GetPartnerCountryById;

public record GetPartnerCountryByIdQuery : IQuery<PartnerCountryResponse>
{
    public Guid PartnerCountryId { get; init; }
} 