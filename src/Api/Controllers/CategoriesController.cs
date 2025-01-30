using Api.Dtos;
using Api.Modules.Errors;
using Application.Categories.Commands;
using Application.Common.Interfaces.Queries;
using Domain.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

//[Authorize(Roles = "Admin")]
[Route("categories")]
[ApiController]
public class CategoriesController(ISender sender, ICategoryQueries categoryQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await categoryQueries.GetAll(cancellationToken);
        return entities.Select(CategoryDto.FromDomainModel).ToList();
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