using FluentValidation;

namespace Application.Categories.Commands;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).MinimumLength(3).MaximumLength(255);
    }
}