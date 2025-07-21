namespace wfc.referential.Application.Operators.Dtos;

public record DeleteOperatorRequest
{
    /// <summary>
    /// The ID of the Operator to delete.
    /// </summary>
    /// <example>9d805d81-8g38-7d4e-1e0h-81849gg947f8</example>
    public Guid OperatorId { get; init; }
}