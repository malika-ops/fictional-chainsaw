using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ServiceControleAggregate.Exceptions;

public class DuplicateServiceControleException : ConflictException
{
    public DuplicateServiceControleException(Guid serviceId, Guid controleId, Guid channelId)
        : base($"A link for Service '{serviceId}', Controle '{controleId}' and Channel '{channelId}' already exists.") { }
}