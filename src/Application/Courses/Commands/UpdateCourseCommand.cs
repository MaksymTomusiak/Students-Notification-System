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

public record UpdateCourseCommand : IRequest<Either<CourseException, Course>>
{
    public required Guid CourseId { get; init; }
    public required string Name { get; init; }
    public required string ImageUrl { get; init; }
    public required string Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime FinishDate { get; init; }
    public required IReadOnlyList<Guid> CategoriesIds { get; init; }
}

public class UpdateCourseCommandHandler(
    ICourseRepository courseRepository,
    ICourseQueries courseQueries,
    ICourseCategoryRepository courseCategoryRepository,
    ICourseCategoryQueries courseCategoryQueries,
    ICategoryQueries categoryQueries) : IRequestHandler<UpdateCourseCommand, Either<CourseException, Course>>
{
    public async Task<Either<CourseException, Course>> Handle(
        UpdateCourseCommand request,
        CancellationToken cancellationToken)
    {
        var existingCourse = await courseQueries.GetById(new CourseId(request.CourseId), cancellationToken);
        
        return await existingCourse.Match(
            async ec =>
            {
                var duplicatedCourse = await CheckDuplicates(ec, cancellationToken);
                
                return await duplicatedCourse.Match(
                    e => Task.FromResult<Either<CourseException, Course>>(new CourseAlreadyExistsException(e.Id)),
                    async () => await UpdateEntity(ec, 
                        request.Name,
                        request.ImageUrl,
                        request.Description,
                        request.StartDate,
                        request.FinishDate,
                        request.CategoriesIds,
                        cancellationToken));
            },
            () => Task.FromResult<Either<CourseException, Course>>(new CourseNotFoundException(new CourseId(request.CourseId))));
    }

    private async Task<Either<CourseException, Course>> UpdateEntity(
        Course entity,
        string name,
        string imageUrl,
        string description,
        DateTime startDate,
        DateTime finishDate,
        IReadOnlyList<Guid> newCategoriesIds,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var category in newCategoriesIds)
            {
                var existingCategory = await categoryQueries.GetById(new CategoryId(category), cancellationToken);

                var categoryFound = existingCategory.Match(
                    c => true,
                    () => false);
                
                if (!categoryFound)
                {
                    return new CourseCategoryNotFoundException(CourseId.Empty());
                }
            }
            
            entity.UpdateDetails(name, description, imageUrl, startDate, finishDate);
            
            var oldCategories = (await courseCategoryQueries.GetByCourse(entity.Id, cancellationToken))
                .Select(x => x.CategoryId)
                .ToList();
            
            var categoriesToRemove = new List<CategoryId>();
            var categoriesToAdd = new List<CategoryId>();

            foreach (var newCategoryId in newCategoriesIds)
            {
                if (oldCategories.All(c => c.Value != newCategoryId))
                {
                    categoriesToAdd.Add(new CategoryId(newCategoryId));
                }
            }
            
            foreach (var oldCategory in oldCategories)
            {
                if (newCategoriesIds.All(ti => ti != oldCategory.Value))
                {
                    categoriesToRemove.Add(oldCategory);
                }
            }

            foreach (var categoryId in categoriesToRemove)
            {
                var courseCategory = await courseCategoryQueries.GetByCourseAndCategory(entity.Id, categoryId, cancellationToken);

                await courseCategory.Match(
                    async et => 
                    {
                        await courseCategoryRepository.Delete(et, cancellationToken);
                        return et;
                    },
                    () => Task.FromResult<CourseCategory?>(null)!
                );
            }

            foreach (var categoryId in categoriesToAdd)
            {
                await courseCategoryRepository.Add(CourseCategory.New(CourseCategoryId.New(), entity.Id, categoryId), cancellationToken);
            }
            
            return await courseRepository.Update(entity, cancellationToken);
        }
        catch (Exception exception)
        {
            return new CourseUnknownException(CourseId.Empty(), exception);
        }
    }

    private async Task<Option<Course>> CheckDuplicates(Course entity, CancellationToken cancellationToken)
    {
        var existingCourse = await courseQueries.SearchByName(entity.Name, cancellationToken);
        
        return existingCourse.Match(
            ec => ec.Id == entity.Id ? Option<Course>.None : Option<Course>.Some(ec),
            Option<Course>.None);
    }
}