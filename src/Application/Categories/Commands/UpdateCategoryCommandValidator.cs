using FluentValidation;

namespace Application.Categories.Commands;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x=> x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).MinimumLength(3).MaximumLength(255);
    }
}