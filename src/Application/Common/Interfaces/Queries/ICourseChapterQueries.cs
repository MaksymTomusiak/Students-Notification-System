using Domain.CourseChapters;
using Domain.Courses;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ICourseChapterQueries
{
    Task<IReadOnlyList<CourseChapter>> GetAll(CancellationToken cancellationToken);
    Task<Option<CourseChapter>> GetById(CourseChapterId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseChapter>> GetByCourseId(CourseId id, CancellationToken cancellationToken);
    Task<Option<CourseChapter>> SearchByNameAndCourse(string name, CourseId courseId, CancellationToken cancellationToken);

}