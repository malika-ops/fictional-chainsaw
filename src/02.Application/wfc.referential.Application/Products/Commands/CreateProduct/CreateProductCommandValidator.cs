using FluentValidation;

namespace wfc.referential.Application.Products.Commands.CreateProduct;


public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

    }
}