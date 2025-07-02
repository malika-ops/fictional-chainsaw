using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Commands.CreatePartner;
using wfc.referential.Application.Partners.Commands.DeletePartner;
using wfc.referential.Application.Partners.Commands.PatchPartner;
using wfc.referential.Application.Partners.Commands.UpdatePartner;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Application.Partners.Queries.GetPartnerById;
using wfc.referential.Application.Partners.Queries.GetFiltredPartners;

namespace wfc.referential.API.Features;

public static class PartnerEndpoints
{
    public static void MapPartnerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/partners")
            .WithTags("Partners");

        group.MapPost("/", CreatePartner)
            .WithName("CreatePartner")
            .WithSummary("Create a new Partner")
            .WithDescription("Creates a partner with code, name, person type, and other relevant information.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredPartners)
            .WithName("GetFiltredPartners")
            .WithSummary("Get paginated list of partners")
            .WithDescription("Returns a paginated list of partners. Filters supported: code, name, person type, professional tax number, headquarters city, tax identification number, ICE, status.")
            .Produces<PagedResult<GetPartnersResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{partnerId:guid}", GetPartnerById)
            .WithName("GetPartnerById")
            .WithSummary("Get a Partner by GUID")
            .WithDescription("Retrieves the Partner identified by partnerId.")
            .Produces<GetPartnersResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{partnerId:guid}", UpdatePartner)
            .WithName("UpdatePartner")
            .WithSummary("Update an existing Partner")
            .WithDescription("Updates the partner identified by partnerId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{partnerId:guid}", PatchPartner)
            .WithName("PatchPartner")
            .WithSummary("Partially update a Partner")
            .WithDescription("Updates only the supplied fields for the partner identified by partnerId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{partnerId:guid}", DeletePartner)
            .WithName("DeletePartner")
            .WithSummary("Delete a partner by GUID")
            .WithDescription("Soft-deletes the partner identified by partnerId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreatePartner(
        CreatePartnerRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreatePartnerCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredPartners(
        [AsParameters] GetFiltredPartnersRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredPartnersQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetPartnerById(
        Guid partnerId,
        IMediator mediator)
    {
        var query = new GetPartnerByIdQuery { PartnerId = partnerId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdatePartner(
        Guid partnerId,
        UpdatePartnerRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdatePartnerCommand>();
        command = command with { PartnerId = partnerId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchPartner(
        Guid partnerId,
        PatchPartnerRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchPartnerCommand>();
        command = command with { PartnerId = partnerId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeletePartner(
        Guid partnerId,
        IMediator mediator)
    {
        var command = new DeletePartnerCommand { PartnerId = partnerId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}