using System.ComponentModel.DataAnnotations;
using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Dtos;

public record UpdateOperatorRequest
{
    /// <summary>
    /// A unique code identifier for the Operator.
    /// </summary>
    /// <example>OP001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The identity card number of the Operator.
    /// </summary>
    /// <example>AB123456</example>
    public string IdentityCode { get; init; } = string.Empty;

    /// <summary>
    /// The last name of the Operator.
    /// </summary>
    /// <example>Alami</example>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// The first name of the Operator.
    /// </summary>
    /// <example>Ahmed</example>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the Operator.
    /// </summary>
    /// <example>ahmed.alami@wafacash.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The phone number of the Operator.
    /// </summary>
    /// <example>+212612345678</example>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Whether the Operator is enabled or not.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// The type of the Operator.
    /// </summary>
    /// <example>1</example>
    public OperatorType? OperatorType { get; init; }

    /// <summary>
    /// The ID of the Branch.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? BranchId { get; init; }

    /// <summary>
    /// The ID of the Profile.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? ProfileId { get; init; }
}