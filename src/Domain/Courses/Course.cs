using Domain.Categories;
using Domain.CourseBans;
using Domain.CourseCategories;
using Domain.CourseChapters;
using Domain.Feedbacks;
using Domain.Registers;
using Domain.Users;

namespace Domain.Courses;

public class Course
{
    public CourseId Id { get; }
    public string Name { get; private set; }
    public string ImageUrl { get; private set; }
    public string Description { get; private set; }
    public Guid CreatorId { get; private set; }
    public User? Creator { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime FinishDate { get; private set; }
    public string Language { get; private set; }
    public string Requirements { get; private set; }
    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    public ICollection<Register> Registers { get; set; } = new List<Register>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<CourseBan> CourseBans { get; set; } = new List<CourseBan>();
    public ICollection<CourseChapter> Chapters { get; set; } = new List<CourseChapter>();
    private Course(CourseId id, string name, string imageUrl, string description, Guid creatorId, DateTime startDate, DateTime finishDate, string language, string requirements)
    {
        Id = id;
        Name = name;
        ImageUrl = imageUrl;
        Description = description;
        CreatorId = creatorId;
        StartDate = startDate;
        FinishDate = finishDate;
        Language = language;
        Requirements = requirements;
    }
    
    public static Course New(CourseId id, string name, string imageUrl, string description, Guid creatorId, DateTime startDate, DateTime finishDate, string language, string requirements)
        => new(id, name, imageUrl, description, creatorId, startDate, finishDate, language, requirements);

    public void UpdateDetails(string name, string description, string imageUrl, DateTime startDate, DateTime finishDate, string language, string requirements)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        StartDate = startDate;
        FinishDate = finishDate;
        Language = language;
        Requirements = requirements;
    }
    
    public void SetImageUrl(string imageUrl) 
        => ImageUrl = imageUrl;
}