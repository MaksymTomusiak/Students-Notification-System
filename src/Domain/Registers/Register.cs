using Domain.Courses;
using Domain.Users;

namespace Domain.Registers;

public class Register
{
    public RegisterId Id { get; }
    public DateTime RegisteredAt { get; private set; }
    public CourseId CourseId { get; }
    public Course? Course { get; }
    public Guid UserId { get;}
    public User? User { get; }

    private Register(RegisterId id, DateTime registeredAt, CourseId courseId, Guid userId)
    {
        Id = id;
        RegisteredAt = registeredAt;
        CourseId = courseId;
        UserId = userId;
    }
    
    public static Register New(RegisterId id, DateTime registeredAt, CourseId courseId, Guid userId)
        => new(id, registeredAt, courseId, userId);
}