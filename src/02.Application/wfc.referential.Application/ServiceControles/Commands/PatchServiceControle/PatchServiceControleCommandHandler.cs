using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using wfc.referential.Domain.ServiceControleAggregate.Exceptions;

namespace wfc.referential.Application.ServiceControles.Commands.PatchServiceControle;

public class PatchServiceControleCommandHandler : ICommandHandler<PatchServiceControleCommand, Result<bool>>
{
    private readonly IServiceControleRepository _repo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IControleRepository _controleRepo;
    private readonly IParamTypeRepository _paramRepo;

    public PatchServiceControleCommandHandler(
        IServiceControleRepository repo,
        IServiceRepository serviceRepo,
        IControleRepository controleRepo,
        IParamTypeRepository paramRepo)
    {
        _repo = repo;
        _serviceRepo = serviceRepo;
        _controleRepo = controleRepo;
        _paramRepo = paramRepo;
    }

    public async Task<Result<bool>> Handle(PatchServiceControleCommand cmd, CancellationToken ct)
    {
        var scId = ServiceControleId.Of(cmd.ServiceControleId);

        var link = await _repo.GetByIdAsync(scId, ct)
            ?? throw new ResourceNotFoundException($"ServiceControle '{cmd.ServiceControleId}' not found.");

        ServiceId serviceId;
        ControleId controleId;
        ParamTypeId channelId;

        if (cmd.ServiceId.HasValue)
        {
            serviceId = ServiceId.Of(cmd.ServiceId.Value);
            var service = await _serviceRepo.GetByIdAsync(serviceId, ct)
                ?? throw new ResourceNotFoundException($"Service '{cmd.ServiceId}' not found.");
        }
        else
        {
            serviceId = link.ServiceId;
        }

        if (cmd.ControleId.HasValue)
        {
            controleId = ControleId.Of(cmd.ControleId.Value);
            var controle = await _controleRepo.GetByIdAsync(controleId, ct)
                ?? throw new ResourceNotFoundException($"Controle '{cmd.ControleId}' not found.");
        }
        else
        {
            controleId = link.ControleId;
        }

        if (cmd.ChannelId.HasValue)
        {
            channelId = ParamTypeId.Of(cmd.ChannelId.Value);
            var chennel = await _paramRepo.GetByIdAsync(channelId, ct)
                ?? throw new ResourceNotFoundException($"Channel '{cmd.ChannelId}' not found.");
        }
        else
        {
            channelId = link.ChannelId;
        }

        // Uniqueness check if any key fields changed
        if (cmd.ServiceId is not null || cmd.ControleId is not null ||
            cmd.ChannelId is not null)
        {
            var duplicate = await _repo.GetOneByConditionAsync(
                sc => sc.ServiceId == serviceId &&
                      sc.ControleId == controleId &&
                      sc.ChannelId == channelId,
                ct);

            if (duplicate is not null && duplicate.Id!.Value != cmd.ServiceControleId)
                throw new DuplicateServiceControleException(
                    serviceId.Value, controleId.Value, channelId.Value);
        }

        link.Patch(
            serviceId,
            controleId,
            channelId,
            cmd.ExecOrder,
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}