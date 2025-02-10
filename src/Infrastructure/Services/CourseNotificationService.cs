using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Services;
using Hangfire;

namespace Infrastructure.Services;

public class CourseNotificationService(
    ICourseQueries courseQueries,
    IEmailService emailService) : ICourseNotificationService
{
    public async Task ScheduleCourseNotifications()
    {
        var upcomingCourses = await courseQueries.GetCoursesStartingInDays(CancellationToken.None, 7, 3, 1);

        foreach (var course in upcomingCourses)
        {
            var daysBefore = (course.StartDate - DateTime.UtcNow.Date).Days;

            // Extract registered users (flattening the collection)
            var users = course.Registers.Select(r => r.User).Distinct(); 

            foreach (var user in users)
            {
                if (user is not null)
                {
                    BackgroundJob.Schedule(
                        () => SendEmailNotification(user.Email!, course.Name, daysBefore),
                        GetNotificationTime(course.StartDate, daysBefore));
                }
            }
        }
    }
    private void SendEmailNotification(string email, string courseName, int daysBefore)
    {
        var subject = $"Reminder: {courseName} starts in {daysBefore} day(s)";
        var message = $"Hello, your course '{courseName}' starts in {daysBefore} days. Get ready!";
        emailService.SendEmail(email, subject, message);
    }

    private DateTime GetNotificationTime(DateTime startDate, int daysBefore)
    {
        return startDate.Date.AddDays(-daysBefore).AddHours(9); // Notify at 9 AM
    }
}