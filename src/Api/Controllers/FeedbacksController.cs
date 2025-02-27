using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Feedbacks.Commands;
using Application.Users.Commands;
using Domain.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("feedbacks")]
[ApiController]
public class FeedbacksController(
    ISender sender,
    IFeedbackQueries feedbackQueries) : ControllerBase
{
    [Authorize]
    [HttpPost("add")]
    public async Task<ActionResult<FeedbackDto>> AddFeedback([FromBody] FeedbackCreateDto request)
    {
        var command = new CreateUserFeedbackCommand
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
    [HttpDelete("delete/{feedbackId:guid}")]
    public async Task<ActionResult<FeedbackDto>> DeleteFeedback([FromRoute] Guid feedbackId)
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

    [Authorize(Roles = "Admin")]
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<FeedbackDto>>> GetUserFeedbacks([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var result = await feedbackQueries.GetByUser(userId, cancellationToken);
        return result.Select(FeedbackDto.FromDomainModel).ToList();
    }
    
    [HttpGet("by-course/{courseId:guid}")]
    public async Task<ActionResult<PaginatedResponse<FeedbackDto>>> GetCourseFeedbacks(
        [FromRoute] Guid courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount, _, _) = await feedbackQueries.GetByCourse(new CourseId(courseId), page, pageSize, cancellationToken);

        var paginatedResponse = new PaginatedResponse<FeedbackDto>
        {
            Items = items.Select(FeedbackDto.FromDomainModel).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(paginatedResponse);
    }
}