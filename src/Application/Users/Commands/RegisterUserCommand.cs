using Application.Common.Interfaces;
using Application.Users.Exceptions;
using Domain.Roles;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record RegisterUserCommand : IRequest<Either<UserException, string>>
{
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}

public class RegisterUserCommandHandler(
    IJwtProvider jwtProvider,
    UserManager<User> userManager,
    RoleManager<Role> roleManager) : IRequestHandler<RegisterUserCommand, Either<UserException, string>>
{
    private const string UserRoleName = "User";
    public async Task<Either<UserException, string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingEmailUser = await userManager.FindByEmailAsync(request.Email);
        if (existingEmailUser != null)
        {
            return new UserWithEmailAlreadyExistsException(existingEmailUser.Id);
        }
        
        var existingUserNameUser = await userManager.FindByNameAsync(request.UserName);
        if (existingUserNameUser != null)
        {
            return new UserWithNameAlreadyExistsException(existingUserNameUser.Id);
        }
        
        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new InvalidCredentialsException();
        }
        
        if (!await roleManager.RoleExistsAsync(UserRoleName))
        {
            await roleManager.CreateAsync(new Role {
                Id = Guid.NewGuid(),
                Name = UserRoleName
            });
        }
        await userManager.AddToRoleAsync(user, UserRoleName);

        var role = await roleManager.FindByNameAsync(UserRoleName);
        return jwtProvider.Generate(user, role);
    }
}