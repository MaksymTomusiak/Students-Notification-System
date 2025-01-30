using Application.Categories.Exceptions;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using LanguageExt;
using MediatR;

namespace Application.Categories.Commands;

public record UpdateCategoryCommand : IRequest<Either<CategoryException, Category>>
{
    public required Guid CategoryId { get; init; }
    public required string Name { get; init; }
}

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICategoryQueries categoryQueries) : IRequestHandler<UpdateCategoryCommand, Either<CategoryException, Category>>
{
    public async Task<Either<CategoryException, Category>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var categoryId = new CategoryId(request.CategoryId);
        var category = await categoryQueries.GetById(categoryId, cancellationToken);

        return await category.Match(
            async c =>
            {
                var existingCategory = await CheckDuplicated(categoryId, request.Name, cancellationToken);

                return await existingCategory.Match(
                    ec => Task.FromResult<Either<CategoryException, Category>>(new CategoryAlreadyExistsException(ec.Id)),
                    async () => await UpdateEntity(c, request.Name, cancellationToken));
            },
            () => Task.FromResult<Either<CategoryException, Category>>(new CategoryNotFoundException(categoryId)));
    }

    private async Task<Either<CategoryException, Category>> UpdateEntity(
        Category category,
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            category.UpdateDetails(name);

            return await categoryRepository.Update(category, cancellationToken);
        }
        catch (Exception exception)
        {
            return new CategoryUnknownException(category.Id, exception);
        }
    }

    private async Task<Option<Category>> CheckDuplicated(
        CategoryId categoryId,
        string name,
        CancellationToken cancellationToken)
    {
        var category = await categoryQueries.SearchByName(name, cancellationToken);

        return category.Match(
            c => c.Id == categoryId ? Option<Category>.None : Option<Category>.Some(c),
            Option<Category>.None);
    }
}