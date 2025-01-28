using Domain.Courses;

namespace Application.Common.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<Course> Add(Course course, CancellationToken cancellationToken);
    Task<Course> Update(Course course, CancellationToken cancellationToken);
    Task<Course> Delete(Course course, CancellationToken cancellationToken);
}