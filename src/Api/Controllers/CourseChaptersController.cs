using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.CourseChapters.Commands;
using Domain.CourseChapters;
using Domain.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("course-chapters")]
[ApiController]
public class CourseChaptersController(ISender sender, ICourseChapterQueries courseChapterQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseChapterDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await courseChapterQueries.GetAll(cancellationToken);
        return entities.Select(CourseChapterDto.FromDomainModel).ToList();
    }
    
    [HttpGet("by-course/{courseId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CourseChapterDto>>> GetByCourse([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        var entities = await courseChapterQueries.GetByCourseId(new CourseId(courseId), cancellationToken);
        return entities.Select(CourseChapterDto.FromDomainModel).ToList();
    }

    [HttpGet("{courseChapterId:guid}")]
    public async Task<ActionResult<CourseChapterDto>> GetById([FromRoute] Guid courseChapterId,
        CancellationToken cancellationToken)
    {
        var entity = await courseChapterQueries.GetById(new CourseChapterId(courseChapterId), cancellationToken);
        return entity.Match<ActionResult<CourseChapterDto>>(
            c => CourseChapterDto.FromDomainModel(c),
            () => NotFound());
    }

    [HttpPost("add")]
    public async Task<ActionResult<CourseChapterDto>> Create(
        [FromBody] CourseChapterCreateDto request,
        CancellationToken cancellationToken)
    {
        var input = new CreateCourseChapterCommand
        {
            Name = request.Name,
            CourseId = request.CourseId,
            EstimatedLearningTimeMinutes = request.EstimatedTime
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseChapterDto>>(
            c => CourseChapterDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpPut("update")]
    public async Task<ActionResult<CourseChapterDto>> Update(
        [FromBody] CourseChapterUpdateDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateCourseChapterCommand
        {
            Id = request.Id,
            Name = request.Name,
            EstimatedLearningTimeMinutes = request.EstimatedTime
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseChapterDto>>(
            c => CourseChapterDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }
    
    [HttpPut("update-order")]
    public async Task<ActionResult<bool>> UpdateOrder(
        [FromBody] CourseChaptersUpdateOrderDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateChaptersOrderCommand
        {
            Numbers = request.Numbers.ToList(),
            ChaptersIds = request.ChaptersIds.ToList()
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<bool>>(
            r => Ok(r),
            e => e.ToObjectResult());
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult<CourseChapterDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var input = new DeleteCourseChapterCommand
        {
            Id = id
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseChapterDto>>(
            c => CourseChapterDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }
}