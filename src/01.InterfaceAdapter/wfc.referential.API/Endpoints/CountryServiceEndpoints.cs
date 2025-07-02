using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryServices.Commands.CreateCountryService;
using wfc.referential.Application.CountryServices.Commands.DeleteCountryService;
using wfc.referential.Application.CountryServices.Commands.PatchCountryService;
using wfc.referential.Application.CountryServices.Commands.UpdateCountryService;
using wfc.referential.Application.CountryServices.Dtos;
using wfc.referential.Application.CountryServices.Queries.GetCountryServiceById;
using wfc.referential.Application.CountryServices.Queries.GetFiltredCountryServices;

namespace wfc.referential.API.Endpoints;

public static class CountryServiceEndpoints
{
    public static void MapCountryServiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/country-services")
            .WithTags("CountryServices");

        group.MapPost("/", CreateCountryService)
            .WithName("CreateCountryService")
            .WithSummary("Create a new Country-Service association")
            .WithDescription("Creates an association between a Country and a Service.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCountryServices)
            .WithName("GetFiltredCountryServices")
            .WithSummary("Get paginated list of country-service associations")
            .WithDescription("Returns a paginated list of country-service associations. Filters supported: countryId, serviceId, status.")
            .Produces<PagedResult<GetCountryServicesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{countryServiceId:guid}", GetCountryServiceById)
            .WithName("GetCountryServiceById")
            .WithSummary("Get a CountryService by GUID")
            .WithDescription("Retrieves the CountryService identified by countryServiceId.")
            .Produces<GetCountryServicesResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{countryServiceId:guid}", UpdateCountryService)
            .WithName("UpdateCountryService")
            .WithSummary("Update an existing Country-Service association")
            .WithDescription("Updates the association identified by countryServiceId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{countryServiceId:guid}", PatchCountryService)
            .WithName("PatchCountryService")
            .WithSummary("Partially update a Country-Service association")
            .WithDescription("Updates only the supplied fields for the association identified by countryServiceId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{countryServiceId:guid}", DeleteCountryService)
            .WithName("DeleteCountryService")
            .WithSummary("Delete a country-service association by GUID")
            .WithDescription("Soft-deletes the association identified by countryServiceId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> GetCountryServiceById(
        Guid countryServiceId,
        IMediator mediator)
    {
        var query = new GetCountryServiceByIdQuery { CountryServiceId = countryServiceId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> CreateCountryService(
        CreateCountryServiceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCountryServiceCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCountryServices(
        [AsParameters] GetFiltredCountryServicesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCountryServicesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCountryService(
        Guid countryServiceId,
        UpdateCountryServiceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCountryServiceCommand>();
        command = command with { CountryServiceId = countryServiceId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCountryService(
        Guid countryServiceId,
        PatchCountryServiceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCountryServiceCommand>();
        command = command with { CountryServiceId = countryServiceId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCountryService(
        Guid countryServiceId,
        IMediator mediator)
    {
        var command = new DeleteCountryServiceCommand(countryServiceId);
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}