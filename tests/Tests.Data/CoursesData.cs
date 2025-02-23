using Domain.Courses;

namespace Tests.Data;

public static class CoursesData
{
    public static Course MainCourse(Guid creatorId) => Course.New(CourseId.New(), 
        "Main course", 
        "https://example.com", 
        "Main course test description", 
        creatorId, 
        DateTime.Now + TimeSpan.FromDays(2), 
        DateTime.Now + TimeSpan.FromDays(4),
        "English",
        "Test requirements");
    
    public static Course SecondaryCourse(Guid creatorId) => Course.New(CourseId.New(), 
        "Secondary course", 
        "https://example.com", 
        "Second course test description", 
        creatorId, 
        DateTime.Now + TimeSpan.FromDays(2), 
        DateTime.Now + TimeSpan.FromDays(4),
        "English",
        "Test requirements");
    
    public static Course NewCourse(Guid creatorId, string name) => Course.New(CourseId.New(), 
        name, 
        "https://test.com", 
        $"{name} course test description", 
        creatorId, 
        DateTime.Now + TimeSpan.FromDays(2), 
        DateTime.Now + TimeSpan.FromDays(4),
        "English",
        "Test requirements");
}