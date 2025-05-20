using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Products.Commands.CreateProduct;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.API.Endpoints.Product;

public class CreateProduct(IMediator _mediator) : Endpoint<CreateProductRequest, Guid>
{
    public override void Configure()
    {
        Post("api/products");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create Product";
            s.Response<PagedResult<CreateProductResponse>>(201, "Product Created Successfully");
        });
        Options(o => o.WithTags(EndpointGroups.Products));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var ProductCommand = req.Adapt<CreateProductCommand>();
        var result = await _mediator.Send(ProductCommand, ct);
        await SendAsync(result.Value, 201, ct);
    }
}