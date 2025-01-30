using Application.Categories.Exceptions;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using LanguageExt;
using MediatR;

namespace Application.Categories.Commands;

public record DeleteCategoryCommand : IRequest<Either<CategoryException, Category>>
{
    public required Guid CategoryId { get; init; }
}

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICategoryQueries categoryQueries,
    ICourseCategoryQueries courseCategoryQueries) : IRequestHandler<DeleteCategoryCommand, Either<CategoryException, Category>>
{
    public async Task<Either<CategoryException, Category>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryId = new CategoryId(request.CategoryId);
        var category = await categoryQueries.GetById(categoryId, cancellationToken);

        return await category.Match(
            async c =>
            {
                var categoryCourses = await courseCategoryQueries.GetByCategory(categoryId, cancellationToken);
                if (categoryCourses.Any())
                {
                    return new CategoryHasCoursesException(categoryId);
                }
                return await DeleteEntity(c, cancellationToken);
            },
            () => Task.FromResult<Either<CategoryException, Category>>(new CategoryNotFoundException(categoryId)));
    }

    private async Task<Either<CategoryException, Category>> DeleteEntity(Category category,
        CancellationToken cancellationToken)
    {
        try
        {
            return await categoryRepository.Delete(category, cancellationToken);
        }
        catch (Exception exception)
        {
            return new CategoryUnknownException(category.Id, exception);
        }
    }
}