using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Sectors.Commands.CreateSector;
using wfc.referential.Application.Sectors.Commands.DeleteSector;
using wfc.referential.Application.Sectors.Commands.PatchSector;
using wfc.referential.Application.Sectors.Commands.UpdateSector;
using wfc.referential.Application.Sectors.Dtos;
using wfc.referential.Application.Sectors.Queries.GetFiltredSectors;

namespace wfc.referential.API.Endpoints;

public static class SectorEndpoints
{
    public static void MapSectorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sectors")
            .WithTags("Sectors");

        group.MapPost("/", CreateSector)
            .WithName("CreateSector")
            .WithSummary("Create a new Sector")
            .WithDescription("Creates a sector with the provided code, name, and city association.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredSectors)
            .WithName("GetFiltredSectors")
            .WithSummary("Get paginated list of sectors")
            .WithDescription("Returns a paginated list of sectors. Filters supported: code, name, cityId, status.")
            .Produces<PagedResult<SectorResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{sectorId:guid}", UpdateSector)
            .WithName("UpdateSector")
            .WithSummary("Update an existing Sector")
            .WithDescription("Updates the sector identified by sectorId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{sectorId:guid}", PatchSector)
            .WithName("PatchSector")
            .WithSummary("Partially update a Sector")
            .WithDescription("Updates only the supplied fields for the sector identified by sectorId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapDelete("/{sectorId:guid}", DeleteSector)
            .WithName("DeleteSector")
            .WithSummary("Delete a sector by GUID")
            .WithDescription("Soft-deletes the sector identified by sectorId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400);
    }

    internal static async Task<IResult> CreateSector(
        CreateSectorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateSectorCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/sectors/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredSectors(
        [AsParameters] GetFiltredSectorsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredSectorsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateSector(
        Guid sectorId,
        UpdateSectorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateSectorCommand>();
        command = command with { SectorId = sectorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchSector(
        Guid sectorId,
        PatchSectorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchSectorCommand>();
        command = command with { SectorId = sectorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteSector(
        Guid sectorId,
        IMediator mediator)
    {
        var command = new DeleteSectorCommand(sectorId);
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}