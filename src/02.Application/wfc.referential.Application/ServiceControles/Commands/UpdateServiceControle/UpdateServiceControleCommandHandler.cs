using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using wfc.referential.Domain.ServiceControleAggregate.Exceptions;

namespace wfc.referential.Application.ServiceControles.Commands.UpdateServiceControle;

public class UpdateServiceControleCommandHandler : ICommandHandler<UpdateServiceControleCommand, Result<bool>>
{
    private readonly IServiceControleRepository _repo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IControleRepository _controleRepo;
    private readonly IParamTypeRepository _paramRepo;

    public UpdateServiceControleCommandHandler(
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

    public async Task<Result<bool>> Handle(UpdateServiceControleCommand cmd, CancellationToken ct)
    {
        var scId = ServiceControleId.Of(cmd.ServiceControleId);
        var serviceId = ServiceId.Of(cmd.ServiceId);
        var controleId = ControleId.Of(cmd.ControleId);
        var channelId = ParamTypeId.Of(cmd.ChannelId);

        var link = await _repo.GetByIdAsync(scId, ct)
            ?? throw new ResourceNotFoundException($"ServiceControle '{cmd.ServiceControleId}' not found.");

        var service = await _serviceRepo.GetByIdAsync(serviceId, ct)
            ?? throw new ResourceNotFoundException($"Service '{cmd.ServiceId}' not found.");

        var controle = await _controleRepo.GetByIdAsync(controleId, ct)
            ?? throw new ResourceNotFoundException($"Controle '{cmd.ControleId}' not found.");

        var chennel = await _paramRepo.GetByIdAsync(channelId, ct)
            ?? throw new ResourceNotFoundException($"Channel '{cmd.ChannelId}' not found.");

        // Uniqueness check (Service, Controle, Channel) 
        var duplicate = await _repo.GetOneByConditionAsync(
            sc => sc.ServiceId == serviceId &&
                  sc.ControleId == controleId &&
                  sc.ChannelId == channelId,
            ct);

        if (duplicate is not null && duplicate.Id!.Value != cmd.ServiceControleId)
            throw new DuplicateServiceControleException(serviceId.Value, controleId.Value, channelId.Value);

        link.Update(
            serviceId,
            controleId,
            channelId,
            cmd.ExecOrder,
            cmd.IsEnabled);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}