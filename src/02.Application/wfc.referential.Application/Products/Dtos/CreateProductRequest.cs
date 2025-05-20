namespace wfc.referential.Application.Products.Dtos;
public record CreateProductRequest
{
    /// <summary>Product Code.</summary>
    /// <example>001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Product Name.</summary>
    /// <example>Cash Express</example>
    public string Name { get; init; } = string.Empty;
}
