using Domain.CourseCategories;

namespace Application.Common.Interfaces.Repositories;

public interface ICourseCategoryRepository
{
    Task<CourseCategory> Add(CourseCategory courseCategory, CancellationToken cancellationToken);
    Task<CourseCategory> Update(CourseCategory courseCategory, CancellationToken cancellationToken);
    Task<CourseCategory> Delete(CourseCategory courseCategory, CancellationToken cancellationToken);
}