using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Users.Commands;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces;
using Domain.Roles;
using Infrastructure.Authentication;
using LanguageExt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("users")]
[ApiController]
public class UsersController(
    UserManager<User> userManager,
    ISender sender,
    IRegisterQueries registerQueries,
    IOptions<JwtOptions> jwtOptions,
    IJwtProvider jwtProvider)
    : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.UserName.ToLower().Contains(search.ToLower()) || c.Email.ToLower().Contains(search.ToLower()));
        }
        
        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new List<UserDto>();
        foreach (var user in entities)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(UserDto.FromDomainModel(user, roles));
        }

        var paginatedResponse = new PaginatedResponse<UserDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(paginatedResponse);
    }
    
    

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] UserLoginDto request)
    {
        var command = new LoginUserCommand
        {
            Email = request.Email,
            Password = request.Password
        };
        var result = await sender.Send(command);
        return result.Match(Ok, e => e.ToObjectResult());
    }

    [HttpGet("login/facebook")]
    public IActionResult LoginFacebook()
    {
        var redirectUrl = Url.Action("FacebookCallback", "Users", null, Request.Scheme);
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        Console.WriteLine($"Initiating Facebook login, redirecting to: {redirectUrl}");
        return Challenge(properties, "Facebook");
    }

    [HttpGet("facebook-callback")]
    public async Task<IActionResult> FacebookCallback()
    {
        // Check for OAuth error parameters (e.g., when user cancels)
        var error = Request.Query["error"];
        var errorDescription = Request.Query["error_description"];
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Facebook OAuth error: {error} - {errorDescription}");
            // Redirect back to frontend login with an error query param
            var redirectUrl = $"http://localhost:5173/login?error={Uri.EscapeDataString(error)}&error_description={Uri.EscapeDataString(errorDescription)}";
            return Redirect(redirectUrl);
        }
        
        var result = await HttpContext.AuthenticateAsync("Facebook");
        if (!result.Succeeded)
        {
            // If authentication fails or token is already present, check for token in redirect
            var tokenValue = Request.Query["token"];
            if (!string.IsNullOrEmpty(tokenValue))
            {
                // Token already issued, redirect to frontend directly
                var redirectUrl = $"http://localhost:5173/login?token={Uri.EscapeDataString(tokenValue)}";
                return Redirect(redirectUrl);
            }
            Console.WriteLine("Facebook authentication failed");
            return BadRequest("Facebook authentication failed");
        }

        var claims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
        Console.WriteLine("Facebook claims: " + string.Join(", ", claims));

        var email = result.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        var name = result.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        name = name?.Replace(' ', '_');
        var facebookId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(facebookId))
        {
            Console.WriteLine("No email or Facebook ID provided");
            return BadRequest("Neither email nor Facebook ID provided by Facebook");
        }

        var userEmail = email ?? $"{facebookId}@facebook.com";
        var user = await userManager.FindByEmailAsync(userEmail);

        if (user == null)
        {
            user = new User
            {
                UserName = name ?? facebookId,
                Email = userEmail
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                Console.WriteLine("User creation failed: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return BadRequest(createResult.Errors);
            }
            await userManager.AddToRoleAsync(user, "User");
        }
        var userRole = await userManager.GetRolesAsync(user);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var tokenString = jwtProvider.Generate(user, new Role { Name = userRole.FirstOrDefault() ?? "User" });

        // Redirect to frontend login page with token
        var redirect = $"http://localhost:5173/login?token={Uri.EscapeDataString(tokenString)}";
        Console.WriteLine($"Redirecting to: {redirect}");
        return Redirect(redirect);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] UserRegisterDto request)
    {
        var command = new RegisterUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            UserName = request.UserName
        };
        var result = await sender.Send(command);
        return result.Match(Ok, e => e.ToObjectResult());
    }

    [Authorize]
    [HttpDelete("delete/{userId:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid userId)
    {
        var command = new DeleteUserCommand { UserId = userId };
        var result = await sender.Send(command);
        return result.Match(Ok, e => e.ToObjectResult());
    }

    [Authorize]
    [HttpPost("enroll-on-course/{courseId:guid}")]
    public async Task<ActionResult<RegisterDto>> EnrollInCourse([FromRoute] Guid courseId)
    {
        var command = new EnrollUserInCourseCommand { CourseId = courseId };
        var result = await sender.Send(command);
        return result.Match<ActionResult<RegisterDto>>(
            r => RegisterDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }

    [Authorize]
    [HttpDelete("unregister-from-course/{courseId:guid}")]
    public async Task<ActionResult<RegisterDto>> UnregisterFromCourse([FromRoute] Guid courseId)
    {
        var command = new UnregisterUserFromCourseCommand { CourseId = courseId };
        var result = await sender.Send(command);
        return result.Match<ActionResult<RegisterDto>>(
            r => RegisterDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<ActionResult<UserDto>> Update([FromBody] UserUpdateDto request)
    {
        var command = new UpdateUserCommand
        {
            PhoneNumber = request.PhoneNumber,
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword
        };
        var result = await sender.Send(command);
        return result.Match<ActionResult<UserDto>>(
            u => UserDto.FromDomainModel(u),
            e => e.ToObjectResult());
    }

    [HttpGet("user-registers/paginated/{userId:guid}")]
    public async Task<ActionResult<PaginatedResponse<RegisterDto>>> GetUserRegisters(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount, _, _) = await registerQueries.GetByUserPaginated(userId, page, pageSize, cancellationToken);

        var paginatedResponse = new PaginatedResponse<RegisterDto>
        {
            Items = items.Select(RegisterDto.FromDomainModel).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(paginatedResponse);
    }

    [HttpGet("user-registers/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<RegisterDto>>> GetUserRegisters([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var items = await registerQueries.GetByUser(userId, cancellationToken);

        return Ok(items.Select(RegisterDto.FromDomainModel));
    }
}