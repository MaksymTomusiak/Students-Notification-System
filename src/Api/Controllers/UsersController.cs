using Api.Dtos;
using Api.Modules.Errors;
using Application.Users.Commands;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("users")]
[ApiController]
public class UsersController(UserManager<User> userManager, ISender sender) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = userManager.Users.ToList();
        var result = new List<UserDto>();
        foreach (var user in entities)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(UserDto.FromDomainModel(user, roles));
        }
        
        return result.ToList();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await sender.Send(command);
        return result.Match(
            Ok,
            e => e.ToObjectResult());
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await sender.Send(command);
        return result.Match(
            Ok,
            e => e.ToObjectResult());
    }
    
    [Authorize]
    [HttpDelete("delete/{userId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid userId)
    {
        var command = new DeleteUserCommand
        {
            UserId = userId
        };
        var result = await sender.Send(command);
        return result.Match(
            Ok,
            e => e.ToObjectResult());
    }
}