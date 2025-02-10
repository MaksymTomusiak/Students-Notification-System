using System.Security.Claims;
using Application.Users.Exceptions;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record UpdateUserCommand : IRequest<Either<UserException, User>>
{
    public required string PhoneNumber { get; init; }
    public required string OldPassword { get; init; }
    public required string NewPassword { get; init; }
}

public class UpdateUserCommandHandler(
    IHttpContextAccessor httpContextAccessor,
    UserManager<User> userManager) : IRequestHandler<UpdateUserCommand, Either<UserException, User>>
{
    public async Task<Either<UserException, User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var sessionUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sessionUserId))
        {
            return new UserIdNotFoundException();
        }

        var sessionUser = await userManager.FindByIdAsync(sessionUserId);
        if (sessionUser == null)
        {
            return new UserNotFoundException(new Guid(sessionUserId));
        }

        var result = await userManager.ChangePasswordAsync(sessionUser, request.OldPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            return new InvalidCredentialsException();
        }
        
        sessionUser.PhoneNumber = request.PhoneNumber;
        await userManager.UpdateAsync(sessionUser);
        
        return sessionUser;
    }
}