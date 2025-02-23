using Domain.Courses;
using Domain.CourseSubChapters;

namespace Domain.CourseChapters;

public class CourseChapter
{
    public CourseChapterId Id { get; }
    public CourseId CourseId { get; }
    public Course? Course { get; private set; }
    public string Name { get; private set; }
    public uint EstimatedLearningTimeMinutes { get; private set; }
    public uint Number { get; private set; }
    public ICollection<CourseSubChapter> SubChapters { get; private set; } = new List<CourseSubChapter>();

    private CourseChapter(CourseChapterId id, CourseId courseId, string name, uint estimatedLearningTimeMinutes, uint number)
    {
        Id = id;
        CourseId = courseId;
        Name = name;
        EstimatedLearningTimeMinutes = estimatedLearningTimeMinutes;
        Number = number;
    }

    public static CourseChapter New(CourseChapterId id, CourseId courseId, string name, uint estimatedLearningTimeMinutes, uint number)
        => new(id, courseId, name, estimatedLearningTimeMinutes, number);

    public void UpdateDetails(string name, uint estimatedLearningTimeMinutes, uint number)
    {
        Name = name;
        EstimatedLearningTimeMinutes = estimatedLearningTimeMinutes;
        Number = number;
    }
    
    public void UpdateNumber(uint number) 
        => Number = number;
}