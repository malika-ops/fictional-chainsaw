using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.OperatorAggregate.Exceptions;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Operators.Commands.CreateOperator;

public class CreateOperatorCommandHandler : ICommandHandler<CreateOperatorCommand, Result<Guid>>
{
    private readonly IOperatorRepository _repo;
    private readonly IAgencyRepository _agencyRepository;

    public CreateOperatorCommandHandler(
        IOperatorRepository repo,
        IAgencyRepository agencyRepository)
    {
        _repo = repo;
        _agencyRepository = agencyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOperatorCommand cmd, CancellationToken ct)
    {
        // Check if the code already exists
        var existingByCode = await _repo.GetByConditionAsync(o => o.Code == cmd.Code, ct);
        if (existingByCode.Any())
            throw new OperatorCodeAlreadyExistException(cmd.Code);

        // Check if the identity code already exists
        var existingByIdentityCode = await _repo.GetByConditionAsync(o => o.IdentityCode == cmd.IdentityCode, ct);
        if (existingByIdentityCode.Any())
            throw new ConflictException($"Operator with identity code {cmd.IdentityCode} already exists.");

        // Check if the email already exists
        var existingByEmail = await _repo.GetByConditionAsync(o => o.Email == cmd.Email, ct);
        if (existingByEmail.Any())
            throw new ConflictException($"Operator with email {cmd.Email} already exists.");

        // Validate Branch exists if provided
        if (cmd.BranchId.HasValue)
        {
            var branch = await _agencyRepository.GetByIdAsync(AgencyId.Of(cmd.BranchId.Value), ct);
            if (branch is null)
                throw new ResourceNotFoundException($"Branch with ID {cmd.BranchId.Value} not found");
        }

        // TODO: Validate ProfileId exists if provided
        // La table profile n'existe pas encore
        // if (cmd.ProfileId.HasValue)
        // {
        //     var profile = await _profileRepository.GetByIdAsync(ProfileId.Of(cmd.ProfileId.Value), ct);
        //     if (profile is null)
        //         throw new ResourceNotFoundException($"Profile with ID {cmd.ProfileId.Value} not found");
        // }

        var id = OperatorId.Of(Guid.NewGuid());
        var operatorEntity = Operator.Create(
            id,
            cmd.Code,
            cmd.IdentityCode,
            cmd.LastName,
            cmd.FirstName,
            cmd.Email,
            cmd.PhoneNumber,
            cmd.OperatorType,
            cmd.BranchId);

        await _repo.AddAsync(operatorEntity, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(operatorEntity.Id.Value);
    }
}
