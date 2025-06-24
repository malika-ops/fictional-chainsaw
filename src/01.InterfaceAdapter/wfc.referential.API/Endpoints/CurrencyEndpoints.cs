using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.CreateCurrency;
using wfc.referential.Application.Currencies.Commands.DeleteCurrency;
using wfc.referential.Application.Currencies.Commands.PatchCurrency;
using wfc.referential.Application.Currencies.Commands.UpdateCurrency;
using wfc.referential.Application.Currencies.Dtos;
using wfc.referential.Application.Currencies.Queries.GetFiltredCurrencies;

namespace wfc.referential.API.Endpoints;

public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/currencies")
            .WithTags("Currencies");

        group.MapPost("/", CreateCurrency)
            .WithName("CreateCurrency")
            .WithSummary("Create a new Currency")
            .WithDescription("Creates a currency with the provided code, name, Arabic/English codes, and ISO code.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCurrencies)
            .WithName("GetFiltredCurrencies")
            .WithSummary("Get paginated list of currencies")
            .WithDescription("Returns a paginated list of currencies. Filters supported: code, codeAR, codeEN, name, codeIso, status.")
            .Produces<PagedResult<GetCurrenciesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{currencyId:guid}", UpdateCurrency)
            .WithName("UpdateCurrency")
            .WithSummary("Update an existing Currency")
            .WithDescription("Updates the currency identified by currencyId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{currencyId:guid}", PatchCurrency)
            .WithName("PatchCurrency")
            .WithSummary("Partially update a Currency")
            .WithDescription("Updates only the supplied fields for the currency identified by currencyId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{currencyId:guid}", DeleteCurrency)
            .WithName("DeleteCurrency")
            .WithSummary("Delete a currency by GUID")
            .WithDescription("Soft-deletes the currency identified by currencyId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateCurrency(
        CreateCurrencyRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCurrencyCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCurrencies(
        [AsParameters] GetFiltredCurrenciesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCurrenciesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCurrency(
        Guid currencyId,
        UpdateCurrencyRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCurrencyCommand>();
        command = command with { CurrencyId = currencyId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCurrency(
        Guid currencyId,
        PatchCurrencyRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCurrencyCommand>();
        command = command with { CurrencyId = currencyId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCurrency(
        Guid currencyId,
        IMediator mediator)
    {
        var command = new DeleteCurrencyCommand(currencyId) ;
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}