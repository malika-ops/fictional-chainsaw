using FluentValidation;

namespace wfc.referential.Application.Sectors.Commands.CreateSector;

public class CreateSectorCommandValidator : AbstractValidator<CreateSectorCommand>
{
    public CreateSectorCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty()
            .WithMessage("Sector code is required.");
        RuleFor(x => x.Name).NotEmpty()
            .WithMessage("Sector name is required.");
        RuleFor(x => x.CityId)
            .NotEqual(Guid.Empty)
            .WithMessage("CityId is required.");
    }
}