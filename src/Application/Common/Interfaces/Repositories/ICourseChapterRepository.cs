using Domain.CourseChapters;

namespace Application.Common.Interfaces.Repositories;

public interface ICourseChapterRepository
{
    Task<CourseChapter> Add(CourseChapter courseChapter, CancellationToken cancellationToken);
    Task<CourseChapter> Update(CourseChapter courseChapter, CancellationToken cancellationToken);
    Task<CourseChapter> Delete(CourseChapter courseChapter, CancellationToken cancellationToken);
}