using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Roles;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record LoginUserCommand : IRequest<Either<UserException, string>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public class LoginUserCommandHandler(
    IJwtProvider jwtProvider,
    UserManager<User> userManager) : IRequestHandler<LoginUserCommand, Either<UserException, string>>
{
    public async Task<Either<UserException, string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return new InvalidCredentialsException();
        }

        var roles = await userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? "User";

        var role = new Role { Name = roleName };

        return jwtProvider.Generate(user, role);
    }
}