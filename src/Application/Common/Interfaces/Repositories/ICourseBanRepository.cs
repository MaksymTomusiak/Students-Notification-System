using Domain.CourseBans;

namespace Application.Common.Interfaces.Repositories;

public interface ICourseBanRepository
{
    Task<CourseBan> Add(CourseBan courseBan, CancellationToken cancellationToken);
    Task<CourseBan> Update(CourseBan courseBan, CancellationToken cancellationToken);
    Task<CourseBan> Delete(CourseBan courseBan, CancellationToken cancellationToken);
}