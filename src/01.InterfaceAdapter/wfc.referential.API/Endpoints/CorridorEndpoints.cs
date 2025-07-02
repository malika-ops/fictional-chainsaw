using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Corridors.Commands.CreateCorridor;
using wfc.referential.Application.Corridors.Commands.DeleteCorridor;
using wfc.referential.Application.Corridors.Commands.PatchCorridor;
using wfc.referential.Application.Corridors.Commands.UpdateCorridor;
using wfc.referential.Application.Corridors.Dtos;
using wfc.referential.Application.Corridors.Queries.GetCorridorById;
using wfc.referential.Application.Corridors.Queries.GetFiltredCorridors;

namespace wfc.referential.API.Endpoints;

public static class CorridorEndpoints
{
    public static void MapCorridorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/corridors")
            .WithTags("Corridors");

        group.MapPost("/", CreateCorridor)
            .WithName("CreateCorridor")
            .WithSummary("Create Corridor")
            .WithDescription("Creates a new corridor with the provided information.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCorridors)
            .WithName("GetFiltredCorridors")
            .WithSummary("Get paginated list of corridors")
            .WithDescription("Returns a paginated list of corridors with optional filtering.")
            .Produces<PagedResult<GetCorridorResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{corridorId:guid}", GetCorridorById)
            .WithName("GetCorridorById")
            .WithSummary("Get a Corridor by GUID")
            .WithDescription("Retrieves the Corridor identified by corridorId.")
            .Produces<GetCorridorResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{corridorId:guid}", UpdateCorridor)
            .WithName("UpdateCorridor")
            .WithSummary("Fully update a Corridor's properties")
            .WithDescription("Updates all fields of the specified Corridor ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);

        group.MapPatch("/{corridorId:guid}", PatchCorridor)
            .WithName("PatchCorridor")
            .WithSummary("Partially update a Corridor's properties")
            .WithDescription("Updates only the provided fields of the specified Corridor ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);

        group.MapDelete("/{corridorId:guid}", DeleteCorridor)
            .WithName("DeleteCorridor")
            .WithSummary("Delete a Corridor")
            .WithDescription("Deletes the specified corridor.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> GetCorridorById(
        Guid corridorId,
        IMediator mediator)
    {
        var query = new GetCorridorByIdQuery { CorridorId = corridorId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> CreateCorridor(
        CreateCorridorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCorridorCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCorridors(
        [AsParameters] GetFiltredCorridorsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCorridorsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCorridor(
        Guid corridorId,
        UpdateCorridorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCorridorCommand>();
        command = command with { CorridorId = corridorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCorridor(
        Guid corridorId,
        PatchCorridorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCorridorCommand>();
        command = command with { CorridorId = corridorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCorridor(
        Guid corridorId,
        IMediator mediator)
    {
        var command = new DeleteCorridorCommand { CorridorId = corridorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}