using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Banks.Commands.CreateBank;
using wfc.referential.Application.Banks.Commands.DeleteBank;
using wfc.referential.Application.Banks.Commands.PatchBank;
using wfc.referential.Application.Banks.Commands.UpdateBank;
using wfc.referential.Application.Banks.Dtos;
using wfc.referential.Application.Banks.Queries.GetFiltredBanks;

namespace wfc.referential.API.Endpoints;

public static class BankEndpoints
{
    public static void MapBankEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/banks")
            .WithTags("Banks");

        group.MapPost("/", CreateBank)
            .WithName("CreateBank")
            .WithSummary("Create a new Bank")
            .WithDescription("Creates a bank with the provided code, name, and abbreviation.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredBanks)
            .WithName("GetFiltredBanks")
            .WithSummary("Get paginated list of banks")
            .WithDescription("Returns a paginated list of banks. Filters supported: code, name, abbreviation, status.")
            .Produces<PagedResult<GetBanksResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{bankId:guid}", UpdateBank)
            .WithName("UpdateBank")
            .WithSummary("Update an existing Bank")
            .WithDescription("Updates the bank identified by bankId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{bankId:guid}", PatchBank)
            .WithName("PatchBank")
            .WithSummary("Partially update a Bank")
            .WithDescription("Updates only the supplied fields for the bank identified by bankId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{bankId:guid}", DeleteBank)
            .WithName("DeleteBank")
            .WithSummary("Delete a bank by GUID")
            .WithDescription("Soft-deletes the bank identified by bankId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreateBank(
        CreateBankRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateBankCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredBanks(
        [AsParameters] GetFiltredBanksRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredBanksQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateBank(
        Guid bankId,
        UpdateBankRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateBankCommand>();
        command = command with { BankId = bankId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchBank(
        Guid bankId,
        PatchBankRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchBankCommand>();
        command = command with { BankId = bankId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteBank(
        Guid bankId,
        IMediator mediator)
    {
        var command = new DeleteBankCommand { BankId = bankId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}