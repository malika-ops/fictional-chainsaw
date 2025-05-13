using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record CreateCountryIdentityDocRequest
{
    /// <summary>
    /// The ID of the Country.
    /// </summary>
    public Guid CountryId { get; init; }

    /// <summary>
    /// The ID of the Identity Document.
    /// </summary>
    public Guid IdentityDocumentId { get; init; }
}