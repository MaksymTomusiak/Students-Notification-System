using FluentValidation;

namespace Application.Users.Commands;

public class DeleteUserFeedbackCommandValidator : AbstractValidator<DeleteUserFeedbackCommand>
{
    public DeleteUserFeedbackCommandValidator()
    {
        RuleFor(x => x.FeedbackId).NotEmpty();
    }
}