using Api.Dtos;
using Api.Modules.Errors;
using Application.Categories.Commands;
using Application.Common.Interfaces.Queries;
using Domain.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("categories")]
[ApiController]
public class CategoriesController(ISender sender, ICategoryQueries categoryQueries) : ControllerBase
{
    [HttpGet("paginated")]
    public async Task<ActionResult<PaginatedResponse<CategoryDto>>> GetAllPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, // Add search query parameter
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount, _, _) = await categoryQueries.GetAllPaginated(page, pageSize, search, cancellationToken);

        var paginatedResponse = new PaginatedResponse<CategoryDto>
        {
            Items = items.Select(CategoryDto.FromDomainModel).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(paginatedResponse);
    }
    
    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> GetAllPaginated(CancellationToken cancellationToken)
    {
        var entities = await categoryQueries.GetAll(cancellationToken);

        return entities.Select(CategoryDto.FromDomainModel);
    }

    [HttpGet("{categoryId:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById([FromRoute] Guid categoryId,
        CancellationToken cancellationToken)
    {
        var entity = await categoryQueries.GetById(new CategoryId(categoryId), cancellationToken);
        return entity.Match<ActionResult<CategoryDto>>(
            c => CategoryDto.FromDomainModel(c),
            () => NotFound());
    }

    [HttpPost("add")]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CategoryCreateDto request,
        CancellationToken cancellationToken)
    {
        var input = new CreateCategoryCommand
        {
            Name = request.Name
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CategoryDto>>(
            c => CategoryDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpPut("update")]
    public async Task<ActionResult<CategoryDto>> Update(
        [FromBody] CategoryDto request,
        CancellationToken cancellationToken)
    {
        var input = new UpdateCategoryCommand
        {
            CategoryId = request.Id,
            Name = request.Name
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CategoryDto>>(
            c => CategoryDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var input = new DeleteCategoryCommand
        {
            CategoryId = id
        };
        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<CategoryDto>>(
            c => CategoryDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }
}