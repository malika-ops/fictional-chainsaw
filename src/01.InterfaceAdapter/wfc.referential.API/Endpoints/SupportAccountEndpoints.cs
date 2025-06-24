using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.SupportAccounts.Commands.CreateSupportAccount;
using wfc.referential.Application.SupportAccounts.Commands.DeleteSupportAccount;
using wfc.referential.Application.SupportAccounts.Commands.PatchSupportAccount;
using wfc.referential.Application.SupportAccounts.Commands.UpdateSupportAccount;
using wfc.referential.Application.SupportAccounts.Commands.UpdateBalance;
using wfc.referential.Application.SupportAccounts.Dtos;
using wfc.referential.Application.SupportAccounts.Queries.GetFiltredSupportAccounts;

namespace wfc.referential.API.Endpoints;

public static class SupportAccountEndpoints
{
    public static void MapSupportAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/support-accounts")
            .WithTags("SupportAccounts");

        group.MapPost("/", CreateSupportAccount)
            .WithName("CreateSupportAccount")
            .WithSummary("Create a new Support Account")
            .WithDescription("Creates a support account with the provided code, description, and other details.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredSupportAccounts)
            .WithName("GetFiltredSupportAccounts")
            .WithSummary("Get paginated list of support accounts")
            .WithDescription("Returns a paginated list of support accounts. Filters supported: code, description, accounting number, partner ID, support account type ID, status.")
            .Produces<PagedResult<GetSupportAccountsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{supportAccountId:guid}", UpdateSupportAccount)
            .WithName("UpdateSupportAccount")
            .WithSummary("Update an existing Support Account")
            .WithDescription("Updates the support account identified by supportAccountId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{supportAccountId:guid}", PatchSupportAccount)
            .WithName("PatchSupportAccount")
            .WithSummary("Partially update a Support Account")
            .WithDescription("Updates only the supplied fields for the support account identified by supportAccountId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapPatch("/{supportAccountId:guid}/balance", UpdateBalance)
            .WithName("UpdateSupportAccountBalance")
            .WithSummary("Update a Support Account's balance")
            .WithDescription("Updates only the balance of the support account identified by supportAccountId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{supportAccountId:guid}", DeleteSupportAccount)
            .WithName("DeleteSupportAccount")
            .WithSummary("Delete a support account by GUID")
            .WithDescription("Soft-deletes the support account identified by supportAccountId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreateSupportAccount(
        CreateSupportAccountRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateSupportAccountCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/support-accounts/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredSupportAccounts(
        [AsParameters] GetFiltredSupportAccountsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredSupportAccountsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateSupportAccount(
        Guid supportAccountId,
        UpdateSupportAccountRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateSupportAccountCommand>();
        command = command with { SupportAccountId = supportAccountId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchSupportAccount(
        Guid supportAccountId,
        PatchSupportAccountRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchSupportAccountCommand>();
        command = command with { SupportAccountId = supportAccountId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> UpdateBalance(
        Guid supportAccountId,
        UpdateSupportAccountBalanceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateSupportAccountBalanceCommand>();
        command = command with { SupportAccountId = supportAccountId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteSupportAccount(
        Guid supportAccountId,
        IMediator mediator)
    {
        var command = new DeleteSupportAccountCommand (supportAccountId);
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}