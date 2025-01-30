using Application.Categories.Exceptions;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using LanguageExt;
using MediatR;

namespace Application.Categories.Commands;

public record CreateCategoryCommand : IRequest<Either<CategoryException, Category>>
{
    public required string Name { get; init; }
}

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICategoryQueries categoryQueries) : IRequestHandler<CreateCategoryCommand, Either<CategoryException, Category>>
{
    public async Task<Either<CategoryException, Category>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var existingCategory = await categoryQueries.SearchByName(request.Name, cancellationToken);

        return await existingCategory.Match(
            c => Task.FromResult<Either<CategoryException, Category>>(new CategoryAlreadyExistsException(c.Id)),
            async () => await CreateEntity(request.Name, cancellationToken));
    }

    private async Task<Either<CategoryException, Category>> CreateEntity(
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            var entity = Category.New(CategoryId.New(), name);

            return await categoryRepository.Add(entity, cancellationToken);
        }
        catch (Exception exception)
        {
            return new CategoryUnknownException(CategoryId.Empty(), exception);
        }
    }
}