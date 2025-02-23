using Domain.CourseChapters;
using Domain.CourseSubChapters;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ICourseSubChapterQueries
{
    Task<IReadOnlyList<CourseSubChapter>> GetAll(CancellationToken cancellationToken);
    Task<Option<CourseSubChapter>> GetById(CourseSubChapterId id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseSubChapter>> GetByCourseChapterId(CourseChapterId id, CancellationToken cancellationToken);
    Task<Option<CourseSubChapter>> SearchByNameAndChapter(string name, CourseChapterId chapterId, CancellationToken cancellationToken);

}