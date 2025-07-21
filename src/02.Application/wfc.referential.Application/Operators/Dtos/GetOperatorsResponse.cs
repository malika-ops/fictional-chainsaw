using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Dtos;

public record GetOperatorsResponse
{
    /// <summary>
    /// Unique identifier of the operator.
    /// </summary>
    /// <example>c1a2b3d4-e5f6-7890-abcd-1234567890ef</example>
    public Guid OperatorId { get; init; }

    /// <summary>
    /// Unique code of the operator.
    /// </summary>
    /// <example>OP001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Identity card number of the operator.
    /// </summary>
    /// <example>AB123456</example>
    public string IdentityCode { get; init; } = string.Empty;

    /// <summary>
    /// Last name of the operator.
    /// </summary>
    /// <example>Alami</example>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// First name of the operator.
    /// </summary>
    /// <example>Ahmed</example>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Email address of the operator.
    /// </summary>
    /// <example>ahmed.alami@wafacash.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Phone number of the operator.
    /// </summary>
    /// <example>+212612345678</example>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the operator is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Type of the operator.
    /// </summary>
    /// <example>Agence</example>
    public OperatorType? OperatorType { get; init; }

    /// <summary>
    /// Branch ID associated with the operator.
    /// </summary>
    /// <example>f7e6d5c4-b3a2-1098-7654-3210fedcba98</example>
    public Guid? BranchId { get; init; }
}