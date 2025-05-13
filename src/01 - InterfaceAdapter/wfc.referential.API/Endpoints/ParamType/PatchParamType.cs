using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.ParamTypes.Commands.PatchParamType;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.API.Endpoints.ParamType;

public class PatchParamType(IMediator _mediator) : Endpoint<PatchParamTypeRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/paramtypes/{ParamTypeId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a ParamType's properties";
            s.Description = "Updates only the provided fields of the specified ParamType ID.";
            s.Params["ParamTypeId"] = "ParamType ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated ParamType");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "ParamType not found");
        });

        Options(o => o.WithTags(EndpointGroups.ParamTypes));
    }

    public override async Task HandleAsync(PatchParamTypeRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchParamTypeCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}