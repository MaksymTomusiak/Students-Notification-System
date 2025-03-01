using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Services;
using Domain.Users;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public class CourseNotificationService(
    ICourseQueries courseQueries,
    UserManager<User> userManager,
    IEmailService emailService,
    ICompositeViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider) : ICourseNotificationService
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

        // Use the application’s IServiceProvider to create a scope and resolve services
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var routeData = new Microsoft.AspNetCore.Routing.RouteData();
        var actionDescriptor = new ActionDescriptor
        {
            DisplayName = "CourseNotification" // Provide a display name for debugging
        };

        // Create a minimal HttpContext with services from the application’s DI
        var httpContext = new DefaultHttpContext
        {
            RequestServices = scopedServices
        };

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        // Generate HTML content using Razor
        var viewResult = viewEngine.FindView(actionContext, "CourseNotification", true); // Search shared locations
        if (viewResult.View == null)
        {
            throw new InvalidOperationException($"Could not find the CourseNotification view. Searched locations: {string.Join(", ", viewResult.SearchedLocations ?? Enumerable.Empty<string>())}");
        }

        using var writer = new StringWriter();

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider), // Use the injected ITempDataProvider
            writer,
            new HtmlHelperOptions()
        );

        // Ensure the ViewContext has access to the application’s services
        viewContext.HttpContext.RequestServices = scopedServices; // Explicitly set RequestServices to ensure all services are available

        try
        {
            viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to render the view: {ex.Message}", ex);
        }

        var htmlBody = writer.ToString();

        emailService.SendEmail(email, subject, htmlBody!, isHtml: true);
    }
}