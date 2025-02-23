using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.CourseSubChapters.Commands;
using Domain.CourseChapters;
using Domain.Courses;
using Domain.CourseSubChapters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("course-subchapters")]
[ApiController]
public class CourseSubChaptersController(ISender sender, ICourseSubChapterQueries courseSubChapterQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseSubChapterDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await courseSubChapterQueries.GetAll(cancellationToken);
        return entities.Select(CourseSubChapterDto.FromDomainModel).ToList();
    }
    
    [HttpGet("by-chapter/{courseChapterId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CourseSubChapterDto>>> GetByCourse([FromRoute] Guid courseChapterId, CancellationToken cancellationToken)
    {
        var entities = await courseSubChapterQueries.GetByCourseChapterId(new CourseChapterId(courseChapterId), cancellationToken);
        return entities.Select(CourseSubChapterDto.FromDomainModel).ToList();
    }

    [HttpGet("{courseSubChapterId:guid}")]
    public async Task<ActionResult<CourseSubChapterDto>> GetById([FromRoute] Guid courseSubChapterId,
        CancellationToken cancellationToken)
    {
        var entity = await courseSubChapterQueries.GetById(new CourseSubChapterId(courseSubChapterId), cancellationToken);
        return entity.Match<ActionResult<CourseSubChapterDto>>(
            c => CourseSubChapterDto.FromDomainModel(c),
            () => NotFound());
    }

    [HttpPost("add")]
    public async Task<ActionResult<CourseSubChapterDto>> Create(
        [FromBody] CourseSubChapterCreateDto request,
        CancellationToken cancellationToken)
    {
        var input = new CreateCourseSubChapterCommand
        {
            Name = request.Name,
            Content = request.Content,
            EstimatedLearningTimeMinutes = request.EstimateTime,
            CourseChapterId = request.ChapterId
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseSubChapterDto>>(
            c => CourseSubChapterDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpPut("update")]
    public async Task<ActionResult<CourseSubChapterDto>> Update(
        [FromBody] CourseSubChapterUpdateDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateCourseSubChapterCommand
        {
            Id = request.Id,
            Name = request.Name,
            EstimatedLearningTimeMinutes = request.EstimateTime,
            Content = request.Content
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseSubChapterDto>>(
            c => CourseSubChapterDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }
    
    [HttpPut("update-order")]
    public async Task<ActionResult<bool>> UpdateOrder(
        [FromBody] CourseSubChaptersUpdateOrderDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateCourseSubChaptersOrderCommand
        {
            ChaptersIds = request.SubChaptersIds.ToList(),
            Numbers = request.Numbers.ToList()
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<bool>>(
            r => Ok(r),
            e => e.ToObjectResult());
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult<CourseSubChapterDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var input = new DeleteCourseSubChapterCommand
        {
            Id = id
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseSubChapterDto>>(
            c => CourseSubChapterDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }
}