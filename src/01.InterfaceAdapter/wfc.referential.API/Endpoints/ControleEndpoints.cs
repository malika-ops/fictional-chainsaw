using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Controles.Commands.CreateControle;
using wfc.referential.Application.Controles.Commands.DeleteControle;
using wfc.referential.Application.Controles.Commands.PatchControle;
using wfc.referential.Application.Controles.Commands.UpdateControle;
using wfc.referential.Application.Controles.Dtos;
using wfc.referential.Application.Controles.Queries.GetControleById;
using wfc.referential.Application.Controles.Queries.GetFilteredControles;


namespace wfc.referential.API.Endpoints;

public static class ControleEndpoints
{
    public static void MapControleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/controles")
                       .WithTags("Controles");

        group.MapPost("/", CreateControle)
             .WithName("CreateControle")
             .WithSummary("Create a new Controle")
             .WithDescription("Creates a Controle with a unique Code and Name.")
             .Produces<Guid>(201)
             .ProducesValidationProblem(400)
             .ProducesProblem(409)
             .ProducesProblem(500);

        group.MapGet("/", GetFiltredControles)
             .WithName("GetFiltredControles")
             .WithSummary("Get paginated list of Controles")
             .WithDescription("Returns a paginated list of Controles. Optional filters: Code, Name, IsEnabled.")
             .Produces<PagedResult<GetControleResponse>>(200)
             .ProducesValidationProblem(400)
             .ProducesProblem(500);

        group.MapGet("/{controleId:guid}", GetControleById)
             .WithName("GetControleById")
             .WithSummary("Get a Controle by GUID")
             .WithDescription("Retrieves the Controle identified by controleId.")
             .Produces<GetControleResponse>(200)
             .Produces(404)
             .ProducesValidationProblem(400);

        group.MapPut("/{controleId:guid}", UpdateControle)
             .WithName("UpdateControle")
             .WithSummary("Update an existing Controle")
             .WithDescription("Replaces Code, Name, and status for the Controle identified by controleId.")
             .Produces<bool>(200)
             .ProducesValidationProblem(400)
             .ProducesProblem(409)
             .ProducesProblem(500);

        group.MapPatch("/{controleId:guid}", PatchControle)
             .WithName("PatchControle")
             .WithSummary("Partially update a Controle")
             .WithDescription("Updates only the supplied fields (Code, Name, IsEnabled) for the Controle identified by controleId.")
             .Produces<bool>(200)
             .ProducesValidationProblem(400)
             .Produces(404)
             .ProducesProblem(409);

        group.MapDelete("/{controleId:guid}", DeleteControle)
             .WithName("DeleteControle")
             .WithSummary("Delete a Controle by GUID")
             .WithDescription("Soft-deletes the Controle identified by controleId (IsEnabled = false).")
             .Produces<bool>(200)
             .ProducesValidationProblem(400);
    }


    internal static async Task<IResult> CreateControle(
        CreateControleRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateControleCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/controles/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredControles(
        [AsParameters] GetFilteredControlesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFilteredControlesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetControleById(
        Guid controleId,
        IMediator mediator)
    {
        var query = new GetControleByIdQuery { ControleId = controleId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateControle(
        Guid controleId,
        UpdateControleRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateControleCommand>() with { ControleId = controleId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchControle(
        Guid controleId,
        PatchControleRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchControleCommand>() with { ControleId = controleId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteControle(
        Guid controleId,
        IMediator mediator)
    {
        var command = new DeleteControleCommand { ControleId = controleId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}
