using Domain.CourseChapters;

namespace Domain.CourseSubChapters;

public class CourseSubChapter
{
    public CourseSubChapterId Id { get; }
    public CourseChapterId CourseChapterId { get; }
    public CourseChapter? CourseChapter { get; private set; }
    public string Name { get; private set; }
    public string Content { get; private set; }
    public uint EstimatedLearningTimeMinutes { get; private set; }
    public uint Number { get; private set; }

    private CourseSubChapter(CourseSubChapterId id, CourseChapterId courseChapterId, string name, string content, uint estimatedLearningTimeMinutes, uint number)
    {
        Id = id;
        CourseChapterId = courseChapterId;
        Name = name;
        Content = content;
        EstimatedLearningTimeMinutes = estimatedLearningTimeMinutes;
        Number = number;
    }

    public static CourseSubChapter New(CourseSubChapterId id, CourseChapterId courseChapterId, string name, string content, uint estimatedLearningTimeMinutes, uint number)
        => new(id, courseChapterId, name, content, estimatedLearningTimeMinutes, number);

    public void UpdateDetails(string name, string content, uint estimatedLearningTimeMinutes, uint number)
    {
        Name = name;
        Content = content;
        EstimatedLearningTimeMinutes = estimatedLearningTimeMinutes;
        Number = number;
    }
    
    public void UpdateNumber(uint number)
        => Number = number;
}