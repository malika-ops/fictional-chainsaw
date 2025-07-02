using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;
using wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;
using wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;
using wfc.referential.Application.PartnerCountries.Dtos;
using wfc.referential.Application.PartnerCountries.Queries.GetFiltredPartnerCountries;
using wfc.referential.Application.PartnerCountries.Queries.GetPartnerCountryById;

namespace wfc.referential.API.Endpoints;

public static class PartnerCountryEndpoints
{
    public static void MapPartnerCountryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/partner-countries")
            .WithTags("PartnerCountries");

        group.MapPost("/", CreatePartnerCountry)
            .WithName("CreatePartnerCountry")
            .WithSummary("Create a Partner-Country link")
            .WithDescription("Creates a new row in `PartnerCountries` that links a Partner to a Country and returns the generated identifier. The (PartnerId, CountryId) pair must be unique.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredPartnerCountries)
            .WithName("GetFiltredPartnerCountries")
            .WithSummary("Get paginated Partner–Country mappings")
            .WithDescription("Returns a paginated list of PartnerCountry rows. Supports optional filtering by PartnerId, CountryId and IsEnabled.")
            .Produces<PagedResult<PartnerCountryResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{partnerCountryId:guid}", GetPartnerCountryById)
            .WithName("GetPartnerCountryById")
            .WithSummary("Get a PartnerCountry by GUID")
            .WithDescription("Retrieves the PartnerCountry identified by partnerCountryId.")
            .Produces<PartnerCountryResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{partnerCountryId:guid}", UpdatePartnerCountry)
            .WithName("UpdatePartnerCountry")
            .WithSummary("Update an existing Partner–Country link")
            .WithDescription("Re-assigns the link or toggles IsEnabled.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapDelete("/{partnerCountryId:guid}", DeletePartnerCountry)
            .WithName("DeletePartnerCountry")
            .WithSummary("Soft-delete a Partner–Country mapping")
            .WithDescription("Sets IsEnabled = false on the specified PartnerCountry.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> GetPartnerCountryById(
        Guid partnerCountryId,
        IMediator mediator)
    {
        var query = new GetPartnerCountryByIdQuery { PartnerCountryId = partnerCountryId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> CreatePartnerCountry(
        CreatePartnerCountryRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreatePartnerCountryCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredPartnerCountries(
        [AsParameters] GetFiltredPartnerCountriesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredPartnerCountriesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdatePartnerCountry(
        Guid partnerCountryId,
        UpdatePartnerCountryRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdatePartnerCountryCommand>();
        command = command with { PartnerCountryId = partnerCountryId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeletePartnerCountry(
        Guid partnerCountryId,
        IMediator mediator)
    {
        var command = new DeletePartnerCountryCommand { PartnerCountryId = partnerCountryId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}