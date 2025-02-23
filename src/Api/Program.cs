using Api.Modules;
using Api.OptionsSetup;
using Application;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Hangfire;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Configure application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.SetupServices();
builder.Services.AddHttpContextAccessor();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT options
builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin",
        options => options.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Authentication setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Use JWT for API auth
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // For OAuth flows
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Default to cookies, not Facebook
})
.AddCookie(options =>
{
    options.LoginPath = "/users/login"; // Redirect path for non-API unauthenticated requests
    options.Events.OnRedirectToLogin = context =>
    {
        // Return 401 for API requests instead of redirecting
        if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/users"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
})
.AddFacebook(options =>
{
    options.ClientId = "1939861403417016";
    options.ClientSecret = "9d8c962a880cd50582aa83b56a615fb2";
    options.CallbackPath = "/signin-facebook";
    options.Scope.Add("email");
    options.Fields.Add("name");
    options.Fields.Add("email");
    options.SaveTokens = true;

    options.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"Remote failure: {context.Failure?.Message} - {context.Failure?.StackTrace}");
        context.HandleResponse();
        // Redirect to frontend login with error
        var error = context.Failure?.Message ?? "Login cancelled or failed";
        var redirectUrl = $"http://localhost:5173/login?error={Uri.EscapeDataString("access_denied")}&error_description={Uri.EscapeDataString(error)}";
        context.Response.Redirect(redirectUrl);
        return Task.CompletedTask;
    };

    options.Events.OnTicketReceived = context =>
    {
        Console.WriteLine("Ticket received from Facebook");
        return Task.CompletedTask;
    };

    options.Events.OnCreatingTicket = async context =>
    {
        var userInfo = context.Identity;
        Console.WriteLine($"User authenticated: {userInfo.Name}");
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigin");
app.UseHttpsRedirection();

var isTesting = builder.Configuration.GetValue<bool>("IsTesting");

if (!isTesting)
{
    app.UseHangfireDashboard();
    RecurringJob.AddOrUpdate<CourseNotificationService>(
        "schedule-course-notifications",
        service => service.ScheduleCourseNotifications(),
        Cron.Daily
    );
}

app.UseAuthentication();
app.UseAuthorization();

await app.InitializeDb();
app.MapControllers();

app.Run();

public partial class Program;