using wfc.referential.Domain.OperatorAggregate;

namespace wfc.referential.Application.Operators.Dtos;

public record PatchOperatorRequest
{
    /// <summary>
    /// If provided, updates the code. If omitted, code remains unchanged.
    /// </summary>
    /// <example>OP002</example>
    public string? Code { get; init; }

    /// <summary>
    /// If provided, updates the identity code. If omitted, identity code remains unchanged.
    /// </summary>
    /// <example>CD789012</example>
    public string? IdentityCode { get; init; }

    /// <summary>
    /// If provided, updates the last name. If omitted, last name remains unchanged.
    /// </summary>
    /// <example>Benali</example>
    public string? LastName { get; init; }

    /// <summary>
    /// If provided, updates the first name. If omitted, first name remains unchanged.
    /// </summary>
    /// <example>Fatima</example>
    public string? FirstName { get; init; }

    /// <summary>
    /// If provided, updates the email. If omitted, email remains unchanged.
    /// </summary>
    /// <example>fatima.benali@wafacash.com</example>
    public string? Email { get; init; }

    /// <summary>
    /// If provided, updates the phone number. If omitted, phone number remains unchanged.
    /// </summary>
    /// <example>+212698765432</example>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// If provided, updates the enabled status. If omitted, enabled status remains unchanged.
    /// </summary>
    /// <example>false</example>
    public bool? IsEnabled { get; init; }

    /// <summary>
    /// If provided, updates the operator type. If omitted, operator type remains unchanged.
    /// </summary>
    /// <example>2</example>
    public OperatorType? OperatorType { get; init; }

    /// <summary>
    /// If provided, updates the branch. If omitted, branch remains unchanged.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? BranchId { get; init; }
}