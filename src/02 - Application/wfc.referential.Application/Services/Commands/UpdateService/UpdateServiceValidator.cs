using FluentValidation;

namespace wfc.referential.Application.Services.Commands.UpdateService;

public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {

        RuleFor(x => x.ServiceId)
            .NotEqual(Guid.Empty).WithMessage("RegionId cannot be empty.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.IsEnabled)
            .NotEmpty().WithMessage("IsEnabled is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");
    }
}
