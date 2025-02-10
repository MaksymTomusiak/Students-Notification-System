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
    UserManager<User> userManager,
    RoleManager<Role> roleManager) : IRequestHandler<LoginUserCommand, Either<UserException, string>>
{
    const string UserRoleName = "User";
    public async Task<Either<UserException, string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return new InvalidCredentialsException();
        }

        var roles = await userManager.GetRolesAsync(user);
        
        if (roles.Count == 0)
        {
            var existingUserRole = await roleManager.FindByNameAsync(UserRoleName);
            if (existingUserRole == null)
            {
                await roleManager.CreateAsync(new Role { Name = UserRoleName });
            }
            
            await userManager.AddToRoleAsync(user, UserRoleName);
        }
        
        var roleName = roles.FirstOrDefault() ?? UserRoleName;

        var role = new Role { Name = roleName };

        return jwtProvider.Generate(user, role);
    }
}