using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Users.Commands;
using Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("users")]
[ApiController]
public class UsersController(UserManager<User> userManager, ISender sender, IRegisterQueries registerQueries) : ControllerBase
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
    public async Task<ActionResult> Login([FromBody] UserLoginDto request)
    {
        var command = new LoginUserCommand
        {
            Email = request.Email,
            Password = request.Password
        };
        
        var result = await sender.Send(command);
        return result.Match(
            Ok,
            e => e.ToObjectResult());
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
        return result.Match(
            Ok,
            e => e.ToObjectResult());
    }
    
    [Authorize]
    [HttpDelete("delete/{userId:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid userId)
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
    
    [Authorize]
    [HttpPost("enroll-on-course/{courseId:guid}")]
    public async Task<ActionResult<RegisterDto>> EnrollInCourse([FromRoute] Guid courseId)
    {
        var command = new EnrollUserInCourseCommand
        {
            CourseId = courseId
        };
        var result = await sender.Send(command);
        return result.Match<ActionResult<RegisterDto>>(
            r => RegisterDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }
    
    [Authorize]
    [HttpDelete("unregister-from-course/{courseId:guid}")]
    public async Task<ActionResult<RegisterDto>> UnregisterFromCourse([FromRoute] Guid courseId)
    {
        var command = new UnregisterUserFromCourseCommand()
        {
            CourseId = courseId
        };
        var result = await sender.Send(command);
        return result.Match<ActionResult<RegisterDto>>(
            r => RegisterDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }
    
    [Authorize]
    [HttpPost("add-feedback")]
    public async Task<ActionResult<FeedbackDto>> AddFeedback([FromBody] FeedbackCreateDto request)
    {
        var command = new CreateUserFeedbackCommand()
        {
            CourseId = request.CourseId,
            Rating = request.Rating,
            Content = request.Content
        };
        var result = await sender.Send(command);
        return result.Match<ActionResult<FeedbackDto>>(
            r => FeedbackDto.FromDomainModel(r),
            e => e.ToObjectResult());
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-user-feedback/{feedbackId:guid}")]
    public async Task<ActionResult<FeedbackDto>> AddFeedback([FromRoute] Guid feedbackId)
    {
        var command = new DeleteUserFeedbackCommand
        {
            FeedbackId = feedbackId
        };
        var result = await sender.Send(command);
        return result.Match<ActionResult<FeedbackDto>>(
            r => FeedbackDto.FromDomainModel(r),
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
    
    [HttpGet("user-registers/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<RegisterDto>>> GetUserRegisters([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var result = await registerQueries.GetByUser(userId, cancellationToken);
        return result.Select(RegisterDto.FromDomainModel).ToList();
    }
}