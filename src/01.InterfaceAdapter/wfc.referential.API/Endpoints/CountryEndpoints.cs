using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Countries.Commands.CreateCountry;
using wfc.referential.Application.Countries.Commands.DeleteCountry;
using wfc.referential.Application.Countries.Commands.PatchCountry;
using wfc.referential.Application.Countries.Commands.UpdateCountry;
using wfc.referential.Application.Countries.Dtos;
using wfc.referential.Application.Countries.Queries.GetCountryById;
using wfc.referential.Application.Countries.Queries.GetFiltredCounties;

namespace wfc.referential.API.Endpoints;

public static class CountryEndpoints
{
    public static void MapCountryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/countries")
            .WithTags("Countries");

        group.MapPost("/", CreateCountry)
            .WithName("CreateCountry")
            .WithSummary("Create a new Country")
            .WithDescription("Creates a country with the provided number, name, code, ISO2, ISO3, dialing code, time zone, status and associated MonetaryZone. Required fields must be provided and valid.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCountries)
            .WithName("GetFiltredCountries")
            .WithSummary("Get paginated list of countries")
            .WithDescription("Returns a paginated list of countries. Supports optional filtering by abbreviation, name, code, ISO2, ISO3, dialing code, time zone, and status.")
            .Produces<PagedResult<GetCountriesResponce>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{countryId:guid}", GetCountryById)
            .WithName("GetCountryById")
            .WithSummary("Get a Country by GUID")
            .WithDescription("Retrieves the Country identified by countryId.")
            .Produces<GetCountriesResponce>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{countryId:guid}", UpdateCountry)
            .WithName("UpdateCountry")
            .WithSummary("Update an existing Country")
            .WithDescription("Updates the country identified by countryId with new fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{countryId:guid}", PatchCountry)
            .WithName("PatchCountry")
            .WithSummary("Partially update a Country")
            .WithDescription("Updates only the supplied fields of the specified Country.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{countryId:guid}", DeleteCountry)
            .WithName("DeleteCountry")
            .WithSummary("Delete a country by GUID")
            .WithDescription("Deletes the Country identified by countryId provided as route parameter.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateCountry(
        CreateCountryRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCountryCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCountries(
        [AsParameters] GetFiltredCountriesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCountriesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetCountryById(
        Guid countryId,
        IMediator mediator)
    {
        var query = new GetCountryByIdQuery { CountryId = countryId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCountry(
        Guid countryId,
        UpdateCountryRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCountryCommand>();
        command = command with { CountryId = countryId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCountry(
        Guid countryId,
        PatchCountryRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCountryCommand>();
        command = command with { CountryId = countryId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCountry(
        Guid countryId,
        IMediator mediator)
    {
        var command = new DeleteCountryCommand { CountryId = countryId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}