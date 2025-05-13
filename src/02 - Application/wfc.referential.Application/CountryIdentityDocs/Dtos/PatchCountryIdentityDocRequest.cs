using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.CountryIdentityDocs.Dtos;

public record PatchCountryIdentityDocRequest
{
    /// <summary>
    /// The ID of the Country Identity Document association to update.
    /// </summary>
    public Guid CountryIdentityDocId { get; init; }

    /// <summary>
    /// The ID of the Country.
    /// </summary>
    public Guid? CountryId { get; init; }

    /// <summary>
    /// The ID of the Identity Document.
    /// </summary>
    public Guid? IdentityDocumentId { get; init; }

    /// <summary>
    /// Indicates if the association is enabled.
    /// </summary>
    public bool? IsEnabled { get; init; }
}