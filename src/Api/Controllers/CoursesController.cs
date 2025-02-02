using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Courses.Commands;
using Domain.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("courses")]
[ApiController]
public class CoursesController(ISender sender, ICourseQueries courseQueries) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await courseQueries.GetAll(cancellationToken);
        return entities.Select(CourseDto.FromDomainModel).ToList();
    }
    
    [HttpGet("{courseId:guid}")]
    public async Task<ActionResult<CourseDto>> GetById([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        var entity = await courseQueries.GetById(new CourseId(courseId), cancellationToken);
        return entity.Match<ActionResult<CourseDto>>(
            e => CourseDto.FromDomainModel(e),
            () => NotFound());
    }
    
    [HttpGet("created-by/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetByCreator([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var entities = await courseQueries.GetByUser(userId, cancellationToken);
        return entities.Select(CourseDto.FromDomainModel).ToList();
    }
    
    [HttpPost("add")]
    public async Task<ActionResult<CourseDto>> Create(
        [FromForm] CourseCreateDto request,
        CancellationToken cancellationToken)
    {
        var input = new CreateCourseCommand
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            FinishDate = request.FinishDate,
            CreatorId = request.CreatorId,
            ImageUrl = request.ImageUrl,
            CategoriesIds = request.CategoriesIds.ToList()
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseDto>>(
            ev => CourseDto.FromDomainModel(ev),
            e => e.ToObjectResult());
    }
    
    [HttpPut("update")]
    public async Task<ActionResult<CourseDto>> Update(
        [FromForm] CourseUpdateDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateCourseCommand
        {
            CourseId = request.Id,
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            FinishDate = request.FinishDate,
            ImageUrl = request.ImageUrl,
            CategoriesIds = request.CategoriesIds.ToList()
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseDto>>(
            ev => CourseDto.FromDomainModel(ev),
            e => e.ToObjectResult());
    }
    
    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult<CourseDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var input = new DeleteCourseCommand
        {
            CourseId = id
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseDto>>(
            ev => CourseDto.FromDomainModel(ev),
            e => e.ToObjectResult());
    }
}