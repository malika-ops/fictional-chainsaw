using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Cities.Commands.CreateCity;
using wfc.referential.Application.Cities.Commands.DeleteCity;
using wfc.referential.Application.Cities.Commands.PatchCity;
using wfc.referential.Application.Cities.Commands.UpdateCity;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Cities.Queries.GetCityById;
using wfc.referential.Application.Cities.Queries.GetFiltredCities;

namespace wfc.referential.API.Endpoints;

public static class CityEndpoints
{
    public static void MapCityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cities")
            .WithTags("Cities");

        group.MapPost("/", CreateCity)
            .WithName("CreateCity")
            .WithSummary("Create city")
            .WithDescription("Creates a new city with the provided information.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCities)
            .WithName("GetFiltredCities")
            .WithSummary("Get paginated list of cities")
            .WithDescription("Returns a paginated list of cities with optional filtering.")
            .Produces<PagedResult<GetCitiyResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{cityId:guid}", GetCityById)
            .WithName("GetCityById")
            .WithSummary("Get a City by GUID")
            .WithDescription("Retrieves the City identified by cityId.")
            .Produces<GetCitiyResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{cityId:guid}", UpdateCity)
            .WithName("UpdateCity")
            .WithSummary("Fully update a City's properties")
            .WithDescription("Updates all fields (code, name, status, countryId) of the specified City ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);

        group.MapPatch("/{cityId:guid}", PatchCity)
            .WithName("PatchCity")
            .WithSummary("Partially update a City properties")
            .WithDescription("Updates only the provided fields (code, name, status or countryId) of the specified City ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);

        group.MapDelete("/{cityId:guid}", DeleteCity)
            .WithName("DeleteCity")
            .WithSummary("Delete a city")
            .WithDescription("Deletes the specified city.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateCity(
        CreateCityRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCityCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCities(
        [AsParameters] GetFiltredCitiesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCitiesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetCityById(
        Guid cityId,
        IMediator mediator)
    {
        var query = new GetCityByIdQuery { CityId = cityId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCity(
        Guid cityId,
        UpdateCityRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCityCommand>();
        command = command with { CityId = cityId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCity(
        Guid cityId,
        PatchCityRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCityCommand>();
        command = command with { CityId = cityId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCity(
        Guid cityId,
        IMediator mediator)
    {
        var command = new DeleteCityCommand { CityId = cityId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}