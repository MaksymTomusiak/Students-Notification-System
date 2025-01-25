using Domain.Courses;
using Domain.Users;

namespace Domain.Feedbacks;

public class Feedback
{
    public FeedbackId Id { get; }
    public DateTime CreatedAt { get; private set; }
    public string Content { get; private set; }
    public ushort Rating { get; private set; }
    public CourseId CourseId { get; }
    public Course? Course { get; }
    public Guid UserId { get;}
    public User? User { get; }

    private Feedback(FeedbackId id, CourseId courseId, Guid userId, DateTime createdAt, string content, ushort rating)
    {
        Id = id;
        CourseId = courseId;
        UserId = userId;
        CreatedAt = createdAt;
        Content = content;
        Rating = rating;
    }
    
    public static Feedback New(FeedbackId id, CourseId courseId, Guid userId, DateTime createdAt, string content, ushort rating)
        => new(id, courseId, userId, createdAt, content, rating);
}