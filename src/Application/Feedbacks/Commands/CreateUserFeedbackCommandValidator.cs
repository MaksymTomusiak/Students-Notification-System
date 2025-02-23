using FluentValidation;

namespace Application.Feedbacks.Commands;

public class CreateUserFeedbackCommandValidator : AbstractValidator<CreateUserFeedbackCommand>
{
    public CreateUserFeedbackCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty()
            .MinimumLength(5)
            .MaximumLength(300);
        RuleFor(x => x.Rating).NotEmpty()
            .GreaterThan((ushort)0)
            .LessThanOrEqualTo((ushort)10);
        RuleFor(x => x.CourseId).NotEmpty();
    }
}