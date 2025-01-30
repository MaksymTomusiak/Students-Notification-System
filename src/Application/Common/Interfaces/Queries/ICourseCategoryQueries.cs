using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ICourseCategoryQueries
{
    Task<IReadOnlyList<CourseCategory>> GetAll(CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseCategory>> GetByCourse(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseCategory>> GetByCategory(CategoryId categoryId, CancellationToken cancellationToken);
    Task<Option<CourseCategory>> GetById(CourseCategoryId id, CancellationToken cancellationToken);
    Task<Option<CourseCategory>> GetByCourseAndCategory(CourseId courseId, CategoryId categoryId, CancellationToken cancellationToken);
}