using Domain.CourseBans;
using Domain.Courses;

namespace Tests.Data;

public static class CourseBansData
{
    public static CourseBan New(CourseId courseId, Guid userId)
        => CourseBan.New(CourseBanId.New(), userId, courseId, "Test reason", DateTime.Now);
}