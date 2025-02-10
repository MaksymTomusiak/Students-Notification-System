using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Feedbacks;

public class FeedbackCreateDtoValidator : AbstractValidator<FeedbackCreateDto>
{
    public FeedbackCreateDtoValidator()
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