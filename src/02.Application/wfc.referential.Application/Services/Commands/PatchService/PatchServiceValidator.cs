using FluentValidation;

namespace wfc.referential.Application.Services.Commands.PatchService;

public class PatchServiceCommandValidator : AbstractValidator<PatchServiceCommand>
{
    public PatchServiceCommandValidator()
    {
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty()
                .WithMessage("Code cannot be empty if provided.");
        });

        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
                .NotEmpty()
                .WithMessage("Name cannot be empty if provided.");
        });

            
        RuleFor(x => x.ServiceId).NotEmpty();
    }
}