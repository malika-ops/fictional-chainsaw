using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Contracts.Commands.CreateContract;
using wfc.referential.Application.Contracts.Commands.DeleteContract;
using wfc.referential.Application.Contracts.Commands.PatchContract;
using wfc.referential.Application.Contracts.Commands.UpdateContract;
using wfc.referential.Application.Contracts.Dtos;
using wfc.referential.Application.Contracts.Queries.GetContractById;
using wfc.referential.Application.Contracts.Queries.GetFiltredContracts;

namespace wfc.referential.API.Endpoints;

public static class ContractEndpoints
{
    public static void MapContractEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/contracts")
            .WithTags("Contracts");

        group.MapPost("/", CreateContract)
            .WithName("CreateContract")
            .WithSummary("Create a new Contract")
            .WithDescription("Creates a contract with code, partner ID, start date, and end date.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredContracts)
            .WithName("GetFiltredContracts")
            .WithSummary("Get paginated list of contracts")
            .WithDescription("Returns a paginated list of contracts. Filters supported: code, partner ID, start date, end date, status.")
            .Produces<PagedResult<GetContractsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{contractId:guid}", GetContractById)
            .WithName("GetContractById")
            .WithSummary("Get a Contract by GUID")
            .WithDescription("Retrieves the Contract identified by contractId.")
            .Produces<GetContractsResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{contractId:guid}", UpdateContract)
            .WithName("UpdateContract")
            .WithSummary("Update an existing Contract")
            .WithDescription("Updates the contract identified by contractId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{contractId:guid}", PatchContract)
            .WithName("PatchContract")
            .WithSummary("Partially update a Contract")
            .WithDescription("Updates only the supplied fields for the contract identified by contractId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{contractId:guid}", DeleteContract)
            .WithName("DeleteContract")
            .WithSummary("Delete a contract by GUID")
            .WithDescription("Soft-deletes the contract identified by contractId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateContract(
        CreateContractRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateContractCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredContracts(
        [AsParameters] GetFiltredContractsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredContractsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetContractById(
        Guid contractId,
        IMediator mediator)
    {
        var query = new GetContractByIdQuery { ContractId = contractId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateContract(
        Guid contractId,
        UpdateContractRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateContractCommand>();
        command = command with { ContractId = contractId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchContract(
        Guid contractId,
        PatchContractRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchContractCommand>();
        command = command with { ContractId = contractId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteContract(
        Guid contractId,
        IMediator mediator)
    {
        var command = new DeleteContractCommand { ContractId = contractId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}