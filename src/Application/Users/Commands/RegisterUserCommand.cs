using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Users.Exceptions;
using Domain.Roles;
using Domain.Users;
using Infrastructure.Services;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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
    RoleManager<Role> roleManager,
    IEmailService emailService,
    IEmailViewRenderer emailViewRenderer)
    : IRequestHandler<RegisterUserCommand, Either<UserException, string>>
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
            UserName = request.UserName,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new InvalidCredentialsException();
        }
        
        if (!await roleManager.RoleExistsAsync(UserRoleName))
        {
            await roleManager.CreateAsync(new Role 
            { 
                Id = Guid.NewGuid(),
                Name = UserRoleName 
            });
        }
        await userManager.AddToRoleAsync(user, UserRoleName);

        // Generate email verification token
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        user.EmailVerificationToken = token;
        user.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

        // Update the user with the verification token
        await userManager.UpdateAsync(user);

        // Send verification email using EmailViewRenderer
        var verificationLink = $"http://localhost:5256/users/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        var model = (UserName: user.UserName, VerificationLink: verificationLink);
        var subject = "Verify Your Email Address";
        var htmlBody = emailViewRenderer.RenderView("EmailVerification", model, user.Email, subject);

        emailService.SendEmail(user.Email, subject, htmlBody, isHtml: true);

        // Return JWT token, but user must verify email before logging in
        var role = await roleManager.FindByNameAsync(UserRoleName);
        return jwtProvider.Generate(user, role);
    }
}