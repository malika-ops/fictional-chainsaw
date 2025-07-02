using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.ContractDetails.Commands.CreateContractDetails;
using wfc.referential.Application.ContractDetails.Commands.DeleteContractDetails;
using wfc.referential.Application.ContractDetails.Commands.PatchContractDetails;
using wfc.referential.Application.ContractDetails.Commands.UpdateContractDetails;
using wfc.referential.Application.ContractDetails.Dtos;
using wfc.referential.Application.ContractDetails.Queries.GetContractDetailById;
using wfc.referential.Application.ContractDetails.Queries.GetFilteredContractDetails;

namespace wfc.referential.API.Endpoints;

public static class ContractDetailsEndpoints
{
    public static void MapContractDetailsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/contractdetails")
            .WithTags("ContractDetails");

        group.MapPost("/", CreateContractDetails)
            .WithName("CreateContractDetails")
            .WithSummary("Create a new ContractDetails")
            .WithDescription("Creates a contract details with contract ID and pricing ID.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFilteredContractDetails)
            .WithName("GetFilteredContractDetails")
            .WithSummary("Get paginated list of contract details")
            .WithDescription("Returns a paginated list of contract details. Filters supported: contract ID, pricing ID, enabled status.")
            .Produces<PagedResult<GetContractDetailsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{contractDetailsId:guid}", GetContractDetailsById)
            .WithName("GetContractDetailsById")
            .WithSummary("Get a ContractDetails by GUID")
            .WithDescription("Retrieves the ContractDetails identified by contractDetailsId.")
            .Produces<GetContractDetailsResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{contractDetailsId:guid}", UpdateContractDetails)
            .WithName("UpdateContractDetails")
            .WithSummary("Update an existing ContractDetails")
            .WithDescription("Updates the contract details identified by contractDetailsId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{contractDetailsId:guid}", PatchContractDetails)
            .WithName("PatchContractDetails")
            .WithSummary("Partially update a ContractDetails")
            .WithDescription("Updates only the supplied fields for the contract details identified by contractDetailsId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{contractDetailsId:guid}", DeleteContractDetails)
            .WithName("DeleteContractDetails")
            .WithSummary("Delete a contract details by GUID")
            .WithDescription("Soft-deletes the contract details identified by contractDetailsId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateContractDetails(
        CreateContractDetailsRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateContractDetailsCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFilteredContractDetails(
        [AsParameters] GetFilteredContractDetailsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFilteredContractDetailsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetContractDetailsById(
        Guid contractDetailsId,
        IMediator mediator)
    {
        var query = new GetContractDetailByIdQuery { ContractDetailsId = contractDetailsId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateContractDetails(
        Guid contractDetailsId,
        UpdateContractDetailsRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateContractDetailsCommand>();
        command = command with { ContractDetailsId = contractDetailsId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchContractDetails(
        Guid contractDetailsId,
        PatchContractDetailsRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchContractDetailsCommand>();
        command = command with { ContractDetailsId = contractDetailsId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteContractDetails(
        Guid contractDetailsId,
        IMediator mediator)
    {
        var command = new DeleteContractDetailsCommand { ContractDetailsId = contractDetailsId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}