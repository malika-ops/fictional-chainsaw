using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.PartnerAccounts.Commands.CreatePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Commands.DeletePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Commands.PatchPartnerAccount;
using wfc.referential.Application.PartnerAccounts.Commands.UpdatePartnerAccount;
using wfc.referential.Application.PartnerAccounts.Commands.UpdateBalance;
using wfc.referential.Application.PartnerAccounts.Dtos;
using wfc.referential.Application.PartnerAccounts.Queries.GetFiltredPartnerAccounts;
using wfc.referential.Application.PartnerAccounts.Queries.GetPartnerAccountById;

namespace wfc.referential.API.Endpoints;

public static class PartnerAccountEndpoints
{
    public static void MapPartnerAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/partner-accounts")
            .WithTags("PartnerAccounts");

        group.MapPost("/", CreatePartnerAccount)
            .WithName("CreatePartnerAccount")
            .WithSummary("Create a new Partner Account")
            .WithDescription("Creates a partner account with account number, RIB, domiciliation, business name, short name, account balance, bank ID and account type.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredPartnerAccounts)
            .WithName("GetFiltredPartnerAccounts")
            .WithSummary("Get paginated list of partner accounts")
            .WithDescription("Returns a paginated list of partner accounts. Filters supported: accountNumber, RIB, businessName, shortName, balance range, bankId, accountTypeId, status.")
            .Produces<PagedResult<PartnerAccountResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{partnerAccountId:guid}", GetPartnerAccountById)
            .WithName("GetPartnerAccountById")
            .WithSummary("Get a PartnerAccount by GUID")
            .WithDescription("Retrieves the PartnerAccount identified by partnerAccountId.")
            .Produces<PartnerAccountResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{partnerAccountId:guid}", UpdatePartnerAccount)
            .WithName("UpdatePartnerAccount")
            .WithSummary("Update an existing Partner Account")
            .WithDescription("Updates the partner account identified by partnerAccountId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{partnerAccountId:guid}", PatchPartnerAccount)
            .WithName("PatchPartnerAccount")
            .WithSummary("Partially update a Partner Account")
            .WithDescription("Updates only the supplied fields for the partner account identified by partnerAccountId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapPatch("/{partnerAccountId:guid}/balance", UpdateBalance)
            .WithName("UpdateBalance")
            .WithSummary("Update a Partner Account's balance")
            .WithDescription("Updates only the balance of the partner account identified by partnerAccountId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{partnerAccountId:guid}", DeletePartnerAccount)
            .WithName("DeletePartnerAccount")
            .WithSummary("Delete a partner account by GUID")
            .WithDescription("Soft-deletes the partner account identified by partnerAccountId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreatePartnerAccount(
        CreatePartnerAccountRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreatePartnerAccountCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredPartnerAccounts(
        [AsParameters] GetFiltredPartnerAccountsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredPartnerAccountsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetPartnerAccountById(
        Guid partnerAccountId,
        IMediator mediator)
    {
        var query = new GetPartnerAccountByIdQuery { PartnerAccountId = partnerAccountId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdatePartnerAccount(
        Guid partnerAccountId,
        UpdatePartnerAccountRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdatePartnerAccountCommand>();
        command = command with { PartnerAccountId = partnerAccountId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchPartnerAccount(
        Guid partnerAccountId,
        PatchPartnerAccountRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchPartnerAccountCommand>();
        command = command with { PartnerAccountId = partnerAccountId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> UpdateBalance(
        Guid partnerAccountId,
        UpdateBalanceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateBalanceCommand>();
        command = command with { PartnerAccountId = partnerAccountId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeletePartnerAccount(
        Guid partnerAccountId,
        IMediator mediator)
    {
        var command = new DeletePartnerAccountCommand (partnerAccountId);
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}