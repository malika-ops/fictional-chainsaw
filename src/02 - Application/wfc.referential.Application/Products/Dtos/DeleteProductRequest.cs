namespace wfc.referential.Application.Products.Dtos;

public record DeleteProductRequest
{
    /// <summary>
    /// The string representation of the Products GUID.
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid ProductID { get; init; }
}
