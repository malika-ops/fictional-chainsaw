using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Products.Commands.PatchProduct;
using wfc.referential.Application.Products.Dtos;


namespace wfc.referential.API.Endpoints.Product;

public class PatchProduct(IMediator _mediator) : Endpoint<PatchProductRequest, bool>
{
    public override void Configure()
    {
        Patch("api/products/{ProductId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Product's properties";
            s.Description = "Updates only the provided fields (code, name, statu or countryId) of the specified Product ID.";
            s.Params["ProductId"] = "Product ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Product");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Product not found");
        });

        Options(o => o.WithTags(EndpointGroups.Products));
    }

    public override async Task HandleAsync(PatchProductRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchProductCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
