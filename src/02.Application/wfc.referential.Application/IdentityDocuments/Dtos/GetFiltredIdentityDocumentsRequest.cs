namespace wfc.referential.Application.IdentityDocuments.Dtos;
public record GetFiltredIdentityDocumentsRequest : FilterRequest
{

    /// <summary>Filter by identity document code.</summary>
    public string? Code { get; init; }

    /// <summary>Filter by identity document name.</summary>
    public string? Name { get; init; }
}   