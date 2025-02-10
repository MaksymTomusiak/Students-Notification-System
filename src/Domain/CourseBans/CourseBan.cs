using Domain.Courses;
using Domain.Users;

namespace Domain.CourseBans;

public class CourseBan
{
    public CourseBanId Id { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; init; }
    public CourseId CourseId { get; private set; }
    public Course? Course { get; init; }
    public string Reason { get; private set; }
    public DateTime BannedAt { get; private set; }

    private CourseBan(CourseBanId id, Guid userId, CourseId courseId, string reason, DateTime bannedAt)
    {
        Id = id;
        UserId = userId;
        CourseId = courseId;
        Reason = reason;
        BannedAt = bannedAt;
    }
    
    public static CourseBan New(CourseBanId id, Guid userId, CourseId courseId, string reason, DateTime bannedAt) 
        => new(id, userId, courseId, reason, bannedAt);
}