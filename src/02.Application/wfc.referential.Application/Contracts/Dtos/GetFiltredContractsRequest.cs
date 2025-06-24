namespace wfc.referential.Application.Contracts.Dtos;

public record GetFiltredContractsRequest : FilterRequest
{
    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by Partner ID.</summary>
    public Guid? PartnerId { get; init; }

    /// <summary>Optional filter by start date.</summary>
    public DateTime? StartDate { get; init; }

    /// <summary>Optional filter by end date.</summary>
    public DateTime? EndDate { get; init; }
}