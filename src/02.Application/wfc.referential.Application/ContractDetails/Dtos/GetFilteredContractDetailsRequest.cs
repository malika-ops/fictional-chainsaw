namespace wfc.referential.Application.ContractDetails.Dtos;

public record GetFilteredContractDetailsRequest : FilterRequest
{
    /// <summary>Optional filter by Contract ID.</summary>
    public Guid? ContractId { get; init; }

    /// <summary>Optional filter by Pricing ID.</summary>
    public Guid? PricingId { get; init; }

    /// <summary>Optional filter by enabled status.</summary>
    public bool? IsEnabled { get; init; }
}