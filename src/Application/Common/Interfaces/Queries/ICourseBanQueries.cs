using Domain.CourseBans;
using Domain.Courses;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ICourseBanQueries
{
    Task<IReadOnlyList<CourseBan>> GetAll(CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseBan>> GetByCourse(CourseId courseId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<CourseBan> Items, int TotalCount, int Page, int PageSize)> GetByUserPaginated(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Option<CourseBan>> GetById(CourseBanId id, CancellationToken cancellationToken);
    Task<Option<CourseBan>> GetByCourseAndUser(CourseId courseId, Guid userId, CancellationToken cancellationToken);
}