using Domain.Courses;
using Domain.Registers;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IRegisterQueries
{
    Task<IReadOnlyList<Register>> GetAll(CancellationToken cancellationToken);
    Task<IReadOnlyList<Register>> GetByCourse(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Register>> GetByUser(Guid userId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Register> Items, int TotalCount, int Page, int PageSize)> GetByUserPaginated(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Option<Register>> GetById(RegisterId id, CancellationToken cancellationToken);
    Task<Option<Register>> GetByCourseAndUser(CourseId courseId, Guid userId, CancellationToken cancellationToken);
}