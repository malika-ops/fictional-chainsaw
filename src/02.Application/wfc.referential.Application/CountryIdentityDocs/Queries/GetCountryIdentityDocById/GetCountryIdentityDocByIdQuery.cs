using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.Application.CountryIdentityDocs.Queries.GetCountryIdentityDocById;

public record GetCountryIdentityDocByIdQuery : IQuery<GetCountryIdentityDocsResponse>
{
    public Guid CountryIdentityDocId { get; init; }
} 