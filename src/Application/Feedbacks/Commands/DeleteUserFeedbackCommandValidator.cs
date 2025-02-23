using FluentValidation;

namespace Application.Feedbacks.Commands;

public class DeleteUserFeedbackCommandValidator : AbstractValidator<DeleteUserFeedbackCommand>
{
    public DeleteUserFeedbackCommandValidator()
    {
        RuleFor(x => x.FeedbackId).NotEmpty();
    }
}