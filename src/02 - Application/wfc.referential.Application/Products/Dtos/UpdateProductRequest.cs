namespace wfc.referential.Application.Products.Dtos;

public record UpdateProductRequest
{
    /// <summary>
    /// The string representation of the Product's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public Guid ProductId { get; set; }

    /// <summary> 
    /// The code of the Product. 
    /// </summary>
    public string Code { get; set; } =string.Empty;

    /// <summary> 
    /// The name of the Product. 
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 
    /// The status of the Product. 
    /// </summary>
    public bool IsEnabled { get; set; }

}
