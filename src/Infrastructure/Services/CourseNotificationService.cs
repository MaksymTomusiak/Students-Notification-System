using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Services;
using Domain.Users;
using Hangfire;
using Microsoft.AspNetCore.Identity;
namespace Infrastructure.Services;

public class CourseNotificationService(
    ICourseQueries courseQueries,
    UserManager<User> userManager,
    IEmailService emailService,
    IEmailViewRenderer emailViewRenderer) : ICourseNotificationService
{
    private readonly int[] _courseStartNotificationDays = [7, 3, 1];
    private const int NotificationHour = 17;

    public async Task ScheduleCourseNotifications()
    {
        var upcomingCourses = await courseQueries.GetCoursesStartingInDays(CancellationToken.None, _courseStartNotificationDays);

        foreach (var course in upcomingCourses)
        {
            var daysBefore = (course.StartDate - DateTime.UtcNow.Date).Days;

            var usersIds = course.Registers.Select(r => r.UserId).Distinct();

            var users = userManager.Users.Where(user => usersIds.Contains(user.Id));
            
            foreach (var user in users)
            {
                var notificationDelay = course.StartDate.Date.AddDays(-daysBefore).AddHours(NotificationHour).AddMinutes(48) - DateTime.UtcNow;
                
                BackgroundJob.Schedule(
                    () => SendEmailNotification(user.Email!, course.Name, daysBefore),
                    notificationDelay);
            }
        }
    }

    public void SendEmailNotification(string email, string courseName, int daysBefore)
    {
        var model = (CourseName: courseName, DaysBefore: daysBefore);
        var subject = $"Reminder: {courseName} starts in {daysBefore} day(s)";

        var htmlBody = emailViewRenderer.RenderView("CourseNotification", model, email, subject);

        emailService.SendEmail(email, subject, htmlBody, isHtml: true);
    }
}