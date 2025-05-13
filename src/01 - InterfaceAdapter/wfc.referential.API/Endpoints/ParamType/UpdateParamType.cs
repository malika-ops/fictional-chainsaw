using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.ParamTypes.Commands.UpdateParamType;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.API.Endpoints.ParamType;

public class UpdateParamType(IMediator _mediator) : Endpoint<UpdateParamTypeRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/paramtypes/{ParamTypeId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing ParamType";
            s.Description = "Updates the ParamType identified by ParamTypeId with new value and other properties.";
            s.Params["ParamTypeId"] = "ParamType ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated ParamType upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the ParamType does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags(EndpointGroups.ParamTypes));
    }

    public override async Task HandleAsync(UpdateParamTypeRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateParamTypeCommand>();
        var paramTypeId = await _mediator.Send(command, ct);
        await SendAsync(paramTypeId, cancellation: ct);
    }
}