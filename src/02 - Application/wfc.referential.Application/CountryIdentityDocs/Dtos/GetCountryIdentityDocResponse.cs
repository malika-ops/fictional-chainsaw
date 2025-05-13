using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record GetCountryIdentityDocResponse(
    Guid CountryIdentityDocId,
    Guid CountryId,
    Guid IdentityDocumentId,
    bool IsEnabled,
    string CountryName,
    string CountryCode,
    string IdentityDocumentName,
    string IdentityDocumentCode
);