using System.ComponentModel.DataAnnotations;

namespace wfc.referential.Application.AgencyTiers.Dtos;

public record PatchAgencyTierRequest
{
    /// <summary>Optional new AgencyId.</summary>
    public Guid? AgencyId { get; init; }

    /// <summary>Optional new TierId.</summary>
    public Guid? TierId { get; init; }

    /// <summary>Optional replacement code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional replacement password.</summary>
    public string? Password { get; init; }

    /// <summary>Optional enabled flag.</summary>
    public bool? IsEnabled { get; init; }
}
