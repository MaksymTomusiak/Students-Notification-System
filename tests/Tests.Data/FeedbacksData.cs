using Domain.Courses;
using Domain.Feedbacks;

namespace Tests.Data;

public static class FeedbacksData
{
    public static Feedback New(Guid userId, CourseId courseId) 
        => Feedback.New(FeedbackId.New(), courseId, userId, DateTime.Now, "Test feedback content", 5);
}