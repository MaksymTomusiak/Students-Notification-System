using Application.Common.Interfaces.Services;
using Application.Users.Exceptions;
using Domain.Users;
using Infrastructure.Services;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Users.Commands;

public record ResendVerificationEmailCommand : IRequest<Either<UserException, bool>>
{
    public required string Email { get; init; }
}

public class ResendVerificationEmailCommandHandler(
    UserManager<User> userManager,
    IEmailService emailService,
    IEmailViewRenderer emailViewRenderer) : IRequestHandler<ResendVerificationEmailCommand, Either<UserException, bool>>
{
    public async Task<Either<UserException, bool>> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new UserNotFoundException(Guid.Empty);
        }

        if (user.EmailConfirmed)
        {
            return true; // Or return a specific message indicating email is already verified
        }

        // Generate a new email verification token
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        user.EmailVerificationToken = token;
        user.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

        await userManager.UpdateAsync(user);
        
        // Send verification email using EmailViewRenderer
        var verificationLink = $"http://localhost:5256/users/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        var model = (UserName: user.UserName, VerificationLink: verificationLink);
        var subject = "Verify Your Email Address";
        var htmlBody = emailViewRenderer.RenderView("ResendEmailVerification", model, user.Email, subject);

        emailService.SendEmail(user.Email, subject, htmlBody, isHtml: true);

        return true;
    }
}