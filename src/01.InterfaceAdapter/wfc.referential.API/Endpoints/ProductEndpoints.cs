using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Products.Commands.CreateProduct;
using wfc.referential.Application.Products.Commands.DeleteProduct;
using wfc.referential.Application.Products.Commands.PatchProduct;
using wfc.referential.Application.Products.Commands.UpdateProduct;
using wfc.referential.Application.Products.Dtos;
using wfc.referential.Application.Products.Queries.GetFiltredProducts;

namespace wfc.referential.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create Product")
            .WithDescription("Creates a new product with the provided code, name, status, and country information.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredProducts)
            .WithName("GetFiltredProducts")
            .WithSummary("Get paginated list of Products")
            .WithDescription("Returns a paginated list of products. Supports optional filtering by code, name, status, and countryId.")
            .Produces<PagedResult<GetFiltredProductsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{productId:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Fully update a Product's properties")
            .WithDescription("Updates all fields (code, name, status, countryId) of the specified Product ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapPatch("/{productId:guid}", PatchProduct)
            .WithName("PatchProduct")
            .WithSummary("Partially update a Product's properties")
            .WithDescription("Updates only the provided fields (code, name, status or countryId) of the specified Product ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{productId:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a Product")
            .WithDescription("Soft-deletes the product identified by productId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);
    }

    internal static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateProductCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/products/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredProducts(
        [AsParameters] GetFiltredProductsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredProductsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateProduct(
        Guid productId,
        UpdateProductRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateProductCommand>();
        command = command with { ProductId = productId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchProduct(
        Guid productId,
        PatchProductRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchProductCommand>();
        command = command with { ProductId = productId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteProduct(
        Guid productId,
        IMediator mediator)
    {
        var command = new DeleteProductCommand { ProductId = productId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}