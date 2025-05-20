using FluentValidation;

namespace wfc.referential.Application.Products.Commands.PatchProduct;

public class PatchProductValidator : AbstractValidator<PatchProductCommand>
{

    public PatchProductValidator()
    {

        // If code is provided (not null), check it's not empty
        When(x => x.Code is not null, () => {
            RuleFor(x => x.Code!)
                .NotEmpty()
                .WithMessage("Code cannot be empty if provided.");
        });

        // If name is provided, check not empty, etc.
        When(x => x.Name is not null, () => {
            RuleFor(x => x.Name!)
            .NotEmpty()
            .WithMessage("Name cannot be empty if provided.");
        });

        // If description is provided
        When(x => x.IsEnabled is not null, () => {
            RuleFor(x => x.IsEnabled!)
            .NotEmpty()
            .WithMessage("IsEnabled cannot be empty if provided.");
        });

    }
}
