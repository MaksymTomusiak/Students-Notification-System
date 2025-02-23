using Domain.CourseSubChapters;

namespace Application.Common.Interfaces.Repositories;

public interface ICourseSubChapterRepository
{
    Task<CourseSubChapter> Add(CourseSubChapter courseSubChapter, CancellationToken cancellationToken);
    Task<CourseSubChapter> Update(CourseSubChapter courseSubChapter, CancellationToken cancellationToken);
    Task<CourseSubChapter> Delete(CourseSubChapter courseSubChapter, CancellationToken cancellationToken);
}