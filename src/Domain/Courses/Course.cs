using Domain.Categories;
using Domain.CourseBans;
using Domain.CourseCategories;
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
    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    public ICollection<Register> Registers { get; set; } = new List<Register>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<CourseBan> CourseBans { get; set; } = new List<CourseBan>();
    private Course(CourseId id, string name, string imageUrl, string description, Guid creatorId, DateTime startDate, DateTime finishDate)
    {
        Id = id;
        Name = name;
        ImageUrl = imageUrl;
        Description = description;
        CreatorId = creatorId;
        StartDate = startDate;
        FinishDate = finishDate;
    }
    
    public static Course New(CourseId id, string name, string imageUrl, string description, Guid creatorId, DateTime startDate, DateTime finishDate)
        => new(id, name, imageUrl, description, creatorId, startDate, finishDate);

    public void UpdateDetails(string name, string description, string imageUrl, DateTime startDate, DateTime finishDate)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        StartDate = startDate;
        FinishDate = finishDate;
    }
}