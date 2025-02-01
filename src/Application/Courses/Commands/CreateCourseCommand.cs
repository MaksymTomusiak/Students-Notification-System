using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Courses.Exceptions;
using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Courses.Commands;

public record CreateCourseCommand : IRequest<Either<CourseException, Course>>
{
    public required string Name { get; init; }
    public required string ImageUrl { get; init; }
    public required string Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime FinishDate { get; init; }
    public required Guid CreatorId { get; init; }
    public required IReadOnlyList<Guid> CategoriesIds { get; init; }
}

public class CreateCourseCommandHandler(
    ICourseRepository courseRepository,
    ICourseQueries courseQueries,
    UserManager<User> userManager,
    ICategoryQueries categoryQueries,
    ICourseCategoryRepository courseCategoryRepository) 
    : IRequestHandler<CreateCourseCommand, Either<CourseException, Course>>
{
    public async Task<Either<CourseException, Course>> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var existingCreator = await userManager.FindByIdAsync(request.CreatorId.ToString());

        if (existingCreator is null)
        {
            return new CourseCreatorNotFoundException(CourseId.Empty());
        }

        var entity = Course.New(CourseId.New(),
            request.Name,
            request.ImageUrl,
            request.Description,
            existingCreator.Id,
            request.StartDate,
            request.FinishDate);

        var existingCourse = await courseQueries.SearchByName(request.Name, cancellationToken);

        return await existingCourse.Match(
            e => Task.FromResult<Either<CourseException, Course>>
                (new CourseAlreadyExistsException(e.Id)),
            async () => await SaveEntity(entity, request.CategoriesIds, cancellationToken));
    }

    private async Task<Either<CourseException, Course>> SaveEntity(
        Course entity,
        IReadOnlyList<Guid> categories,
        CancellationToken cancellationToken)
    {
        try
        {
            var courseCategories = new List<CourseCategory>();
            foreach (var category in categories)
            {
                var existingCategory = await categoryQueries.GetById(new CategoryId(category), cancellationToken);

                var categoryFound = existingCategory.Match(
                    c =>
                    {
                        courseCategories.Add(CourseCategory.New(CourseCategoryId.New(), entity.Id, c.Id ));
                        return true;
                    },
                    () => false);
                
                if (!categoryFound)
                {
                    return new CourseCategoryNotFoundException(CourseId.Empty());
                }
            }
            var result = await courseRepository.Add(entity, cancellationToken);

            foreach (var courseCategory in courseCategories)
            {
                await courseCategoryRepository.Add(courseCategory, cancellationToken);
            }
            
            return result;
        }
        catch (Exception exception)
        {
            return new CourseUnknownException(CourseId.Empty(), exception);
        }
    }
    
    
}