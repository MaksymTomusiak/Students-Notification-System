using FluentValidation;

namespace Application.Users.Commands;

public class ResendVerificationEmailCommandHandlerValidator : AbstractValidator<ResendVerificationEmailCommand>
{
    public ResendVerificationEmailCommandHandlerValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
    }
}