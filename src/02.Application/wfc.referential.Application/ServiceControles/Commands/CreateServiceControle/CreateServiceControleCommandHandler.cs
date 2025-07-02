using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.ControleAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.ServiceControleAggregate;
using wfc.referential.Domain.ServiceControleAggregate.Exceptions;

namespace wfc.referential.Application.ServiceControles.Commands.CreateServiceControle;

public class CreateServiceControleCommandHandler : ICommandHandler<CreateServiceControleCommand, Result<Guid>>
{
    private readonly IServiceControleRepository _repo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IControleRepository _controleRepo;
    private readonly IParamTypeRepository _paramRepo;

    public CreateServiceControleCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateServiceControleCommand cmd, CancellationToken ct)
    {
        var serviceId = ServiceId.Of(cmd.ServiceId);
        var controleId = ControleId.Of(cmd.ControleId);
        var channelId = ParamTypeId.Of(cmd.ChannelId);
        var newId = ServiceControleId.Of(Guid.NewGuid());

        var service = await _serviceRepo.GetByIdAsync(serviceId, ct)
            ?? throw new ResourceNotFoundException($"Service '{cmd.ServiceId}' not found.");

        var controle = await _controleRepo.GetByIdAsync(controleId, ct)
            ?? throw new ResourceNotFoundException($"Controle '{cmd.ControleId}' not found.");

        var channel = await _paramRepo.GetByIdAsync(channelId, ct)
            ?? throw new ResourceNotFoundException($"Channel '{cmd.ChannelId}' not found.");

        var duplicate = await _repo.GetOneByConditionAsync(
            sc => sc.ServiceId == serviceId &&
                  sc.ControleId == controleId &&
                  sc.ChannelId == channelId,
            ct);

        if (duplicate is not null)
            throw new DuplicateServiceControleException(serviceId.Value, controleId.Value, channelId.Value);

        var entity = ServiceControle.Create(
            newId,
            serviceId,
            controleId,
            channelId,
            cmd.ExecOrder);

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(entity.Id!.Value);
    }
}