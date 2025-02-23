using FluentValidation;

namespace Application.CourseChapters.Commands;

public class UpdateChaptersOrderCommandValidator : AbstractValidator<UpdateChaptersOrderCommand>
{
    public UpdateChaptersOrderCommandValidator()
    {
        RuleFor(x => x.ChaptersIds).NotEmpty();
        RuleFor(x => x.ChaptersIds).NotEmpty();
    }
}