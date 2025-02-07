using Domain.Courses;
using Domain.Registers;

namespace Tests.Data;

public static class RegistersData
{
    public static Register New(Guid userId, CourseId courseId)
        => Register.New(RegisterId.New(), DateTime.Now, courseId, userId);
}