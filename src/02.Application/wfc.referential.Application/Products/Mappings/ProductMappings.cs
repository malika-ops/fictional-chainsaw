using Mapster;
using wfc.referential.Application.Products.Commands.CreateProduct;
using wfc.referential.Application.Products.Commands.DeleteProduct;
using wfc.referential.Application.Products.Commands.PatchProduct;
using wfc.referential.Application.Products.Commands.UpdateProduct;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Application.Products.Queries.GetFiltredProducts;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Mappings;

public class ProductMappings
{
    public static void Register()
    {

        TypeAdapterConfig<Product, GetProdcutsResponse>.NewConfig()
            .Map(dest => dest.ProductId, src => src.Id!.Value);

        // Map from ProductId to nullable Guid
        TypeAdapterConfig<ProductId, Guid?>
            .NewConfig()
            .MapWith(src => src == null ? (Guid?)null : src.Value);

        // Map from nullable Guid to ProductId
        TypeAdapterConfig<Guid?, ProductId>
            .NewConfig()
            .MapWith(src => src.HasValue ? ProductId.Of(src.Value) : null);
    }
}
