using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.CurrencyDenominations.Commands.CreateCurrencyDenomination;
using wfc.referential.Application.CurrencyDenominations.Commands.DeleteCurrencyDenomination;
using wfc.referential.Application.CurrencyDenominations.Commands.PatchCurrencyDenomination;
using wfc.referential.Application.CurrencyDenominations.Commands.UpdateCurrencyDenomination;
using wfc.referential.Application.CurrencyDenominations.Dtos;
using wfc.referential.Application.CurrencyDenominations.Queries.GetCurrencyDenominationById;
using wfc.referential.Application.CurrencyDenominations.Queries.GetFiltredCurrencies;

namespace wfc.referential.API.Endpoints;

public static class CurrencyDenominationEndpoints
{
    public static void MapCurrencyDenominationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/currencydenominations")
            .WithTags("CurrencyDenominations");

        group.MapPost("/", CreateCurrencyDenomination)
            .WithName("CreateCurrencyDenomination")
            .WithSummary("Create a new CurrencyDenomination")
            .WithDescription("Creates a currencyDenomination with the provided code, name, Arabic/English codes, and ISO code.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCurrencyDenominations)
            .WithName("GetFiltredCurrencyDenominations")
            .WithSummary("Get paginated list of currencydenominations")
            .WithDescription("Returns a paginated list of currencydenominations. Filters supported: code, codeAR, codeEN, name, codeIso, status.")
            .Produces<PagedResult<GetCurrencyDenominationsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{currencyDenominationId:guid}", GetCurrencyDenominationById)
            .WithName("GetCurrencyDenominationById")
            .WithSummary("Get a Currency Denomination by GUID")
            .WithDescription("Retrieves the Currency Denomination identified by currencyDenominationId.")
            .Produces<GetCurrencyDenominationsResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{currencyDenominationId:guid}", UpdateCurrencyDenomination)
            .WithName("UpdateCurrencyDenomination")
            .WithSummary("Update an existing Currency Denomination")
            .WithDescription("Updates the currency Denomination identified by currencyDenominationId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{currencyDenominationId:guid}", PatchCurrencyDenomination)
            .WithName("PatchCurrencyDenomination")
            .WithSummary("Partially update a Currency Denomination")
            .WithDescription("Updates only the supplied fields for the currency denomination identified by currencyId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{currencyDenominationId:guid}", DeleteCurrencyDenomination)
            .WithName("DeleteCurrencyDenomination")
            .WithSummary("Delete a currency denomination by GUID")
            .WithDescription("Soft-deletes the currency denomination identified by currencyId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateCurrencyDenomination(
        CreateCurrencyDenominationRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCurrencyDenominationCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCurrencyDenominations(
        [AsParameters] GetFiltredCurrencyDenominationsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCurrencyDenominationsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetCurrencyDenominationById(
        Guid currencyDenominationId,
        IMediator mediator)
    {
        var query = new GetCurrencyDenominationByIdQuery { CurrencyDenominationId = currencyDenominationId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCurrencyDenomination(
        Guid currencyDenominationId,
        UpdateCurrencyDenominationRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCurrencyDenominationCommand>();
        command = command with { CurrencyDenominationId = currencyDenominationId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCurrencyDenomination(
        Guid currencyDenominationId,
        PatchCurrencyDenominationRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCurrencyDenominationCommand>();
        command = command with { CurrencyDenominationId = currencyDenominationId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCurrencyDenomination(
        Guid currencyDenominationId,
        IMediator mediator)
    {
        var command = new DeleteCurrencyDenominationCommand(currencyDenominationId) ;
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}