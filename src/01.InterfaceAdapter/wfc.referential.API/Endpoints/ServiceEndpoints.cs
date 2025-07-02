using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Services.Commands.CreateService;
using wfc.referential.Application.Services.Commands.DeleteService;
using wfc.referential.Application.Services.Commands.PatchService;
using wfc.referential.Application.Services.Commands.UpdateService;
using wfc.referential.Application.Services.Dtos;
using wfc.referential.Application.Services.Queries.GetServiceById;
using wfc.referential.Application.Services.Queries.GetFiltredServices;

namespace wfc.referential.API.Endpoints;

public static class ServiceEndpoints
{
    public static void MapServiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/services")
            .WithTags("Services");

        group.MapPost("/", CreateService)
            .WithName("CreateService")
            .WithSummary("Create Service")
            .WithDescription("Creates a new service with the provided code, name, and status information.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredServices)
            .WithName("GetFiltredServices")
            .WithSummary("Get paginated list of Services")
            .WithDescription("Returns a paginated list of services. Supports optional filtering by code, name, and status.")
            .Produces<PagedResult<GetServicesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{serviceId:guid}", GetServiceById)
            .WithName("GetServiceById")
            .WithSummary("Get a Service by GUID")
            .WithDescription("Retrieves the Service identified by serviceId.")
            .Produces<GetServicesResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{serviceId:guid}", UpdateService)
            .WithName("UpdateService")
            .WithSummary("Fully update a Service's properties")
            .WithDescription("Updates all fields (code, name, status) of the specified Service ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapPatch("/{serviceId:guid}", PatchService)
            .WithName("PatchService")
            .WithSummary("Partially update a Service's properties")
            .WithDescription("Updates only the provided fields (code, name, status) of the specified Service ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{serviceId:guid}", DeleteService)
            .WithName("DeleteService")
            .WithSummary("Delete a Service")
            .WithDescription("Soft-deletes the service identified by serviceId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);
    }

    internal static async Task<IResult> CreateService(
        CreateServiceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateServiceCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/services/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredServices(
        [AsParameters] GetFiltredServicesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredServicesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetServiceById(
        Guid serviceId,
        IMediator mediator)
    {
        var query = new GetServiceByIdQuery { ServiceId = serviceId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateService(
        Guid serviceId,
        UpdateServiceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateServiceCommand>();
        command = command with { ServiceId = serviceId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchService(
        Guid serviceId,
        PatchServiceRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchServiceCommand>();
        command = command with { ServiceId = serviceId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteService(
        Guid serviceId,
        IMediator mediator)
    {
        var command = new DeleteServiceCommand { ServiceId = serviceId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}