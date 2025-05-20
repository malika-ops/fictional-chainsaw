using Mapster;
using wfc.referential.Application.Products.Commands.CreateProduct;
using wfc.referential.Application.Products.Commands.DeleteProduct;
using wfc.referential.Application.Products.Commands.PatchProduct;
using wfc.referential.Application.Products.Commands.UpdateProduct;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Application.Products.Queries.GetAllProducts;
using wfc.referential.Domain.ProductAggregate;

namespace wfc.referential.Application.Products.Mappings;

public class ProductMappings
{
    public static void Register()
    {
        // Products
        TypeAdapterConfig<GetAllProductsRequest, GetAllProductsQuery>
        .NewConfig()
        .Map(dest => dest.PageNumber, src => src.PageNumber ?? 1)
        .Map(dest => dest.PageSize, src => src.PageSize ?? 10)
        .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        TypeAdapterConfig<Product, GetAllProductsResponse>.NewConfig()
            .Map(dest => dest.ProductId, src => src.Id!.Value)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled.ToString());

        TypeAdapterConfig<CreateProductRequest, CreateProductCommand>.NewConfig()
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name);

        TypeAdapterConfig<PatchProductRequest, PatchProductCommand>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

        //.MapToConstructor(true)
        //.ConstructUsing(src => new PatchProductCommand(
        //    src.ProductId,
        //    src.Code,
        //    src.Name,
        //    src.Status,
        //    src.CountryId
        //));
        TypeAdapterConfig<PatchProductCommand, Product>
            .NewConfig()
            .IgnoreNullValues(true);

        TypeAdapterConfig<DeleteProductRequest, DeleteProductCommand>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductID);

        TypeAdapterConfig<UpdateProductRequest, UpdateProductCommand>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsEnabled, src => src.IsEnabled);

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
