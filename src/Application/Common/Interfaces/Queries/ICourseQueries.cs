using Domain.Categories;
using Domain.Courses;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ICourseQueries
{
    Task<IReadOnlyList<Course>> GetAll(CancellationToken cancellationToken);
    Task<IReadOnlyList<Course>> GetByUser(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Course>> GetByCategories(IReadOnlyList<CategoryId> categories, CancellationToken cancellationToken);
    Task<IReadOnlyList<Course>> GetByDate(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken);
    Task<IReadOnlyList<Course>> GetCoursesStartingInDays(CancellationToken cancellationToken, params int[] days);
    Task<Option<Course>> GetById(CourseId id, CancellationToken cancellationToken);
    Task<Option<Course>> SearchByName(string name, CancellationToken cancellationToken);
}