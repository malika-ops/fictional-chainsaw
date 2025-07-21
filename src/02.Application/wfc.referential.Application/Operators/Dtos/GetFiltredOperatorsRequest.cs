using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Dtos;

public record GetFiltredOperatorsRequest : FilterRequest
{
    /// <summary>Optional filter by code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by identity code.</summary>
    public string? IdentityCode { get; init; }

    /// <summary>Optional filter by last name.</summary>
    public string? LastName { get; init; }

    /// <summary>Optional filter by first name.</summary>
    public string? FirstName { get; init; }

    /// <summary>Optional filter by email.</summary>
    public string? Email { get; init; }

    /// <summary>Optional filter by operator type.</summary>
    public OperatorType? OperatorType { get; init; }

    /// <summary>Optional filter by branch ID.</summary>
    public Guid? BranchId { get; init; }
}