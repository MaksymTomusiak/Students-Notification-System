using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators.Feedbacks;

public class FeedbackDtoValidator : AbstractValidator<FeedbackDto>
{
    public FeedbackDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.Content).NotEmpty()
            .MinimumLength(5)
            .MaximumLength(300);
        RuleFor(x => x.Rating).NotEmpty()
            .GreaterThan((ushort)0)
            .LessThanOrEqualTo((ushort)10);
    }
}