using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.CourseBans.Commands;
using Domain.CourseBans;
using Domain.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("bans")]
[ApiController]
public class CourseBansController(ISender sender, ICourseBanQueries courseBanQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseBanDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await courseBanQueries.GetAll(cancellationToken);
        return entities.Select(CourseBanDto.FromDomainModel).ToList();
    }
    
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<PaginatedResponse<CourseBanDto>>> GetUserBans(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount, _, _) = await courseBanQueries.GetByUserPaginated(userId, page, pageSize, cancellationToken);

        var paginatedResponse = new PaginatedResponse<CourseBanDto>
        {
            Items = items.Select(CourseBanDto.FromDomainModel).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(paginatedResponse);
    }
    
    [HttpGet("by-course/{courseId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CourseBanDto>>> GetCourseBans(Guid courseId, CancellationToken cancellationToken)
    {
        var entities = await courseBanQueries.GetByCourse(new CourseId(courseId), cancellationToken);
        return entities.Select(CourseBanDto.FromDomainModel).ToList();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseBanDto>> GetById([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await courseBanQueries.GetById(new CourseBanId(id), cancellationToken);
        return entity.Match<ActionResult<CourseBanDto>>(
            c => CourseBanDto.FromDomainModel(c),
            () => NotFound());
    }
    
    [HttpPost("add")]
    public async Task<ActionResult<CourseBanDto>> Create(
        [FromBody] BanDto request,
        CancellationToken cancellationToken)
    {
        var input = new BanUserFromCourseCommand
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            Reason = request.Reason
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseBanDto>>(
            cb => CourseBanDto.FromDomainModel(cb),
            e => e.ToObjectResult());
    }
    
    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult<CourseBanDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var input = new UnbanUserFromCourseCommand
        {
            BanId = id
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CourseBanDto>>(
            cb => CourseBanDto.FromDomainModel(cb),
            e => e.ToObjectResult());
    }
}