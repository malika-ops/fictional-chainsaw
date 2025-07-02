namespace wfc.referential.Application.Products.Dtos;

public record UpdateProductRequest
{
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
