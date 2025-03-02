// Application.Users.Commands/VerifyEmailCommand.cs
using Application.Users.Exceptions;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record VerifyEmailCommand : IRequest<Either<UserException, (bool Success, string? UserName)>>
{
    public required Guid UserId { get; init; }
    public required string Token { get; init; }
}

public class VerifyEmailCommandHandler(
    UserManager<User> userManager) : IRequestHandler<VerifyEmailCommand, Either<UserException, (bool Success, string? UserName)>>
{
    public async Task<Either<UserException, (bool Success, string? UserName)>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return new UserNotFoundException(request.UserId);
        }

        if (user.EmailVerificationTokenExpiration < DateTime.UtcNow)
        {
            return new EmailVerificationTokenExpiredException(request.UserId);
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            return new InvalidVerificationTokenException(request.UserId);
        }

        // Clear the verification token and mark email as confirmed
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiration = null;
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        return (Success: true, UserName: user.UserName);
    }
}