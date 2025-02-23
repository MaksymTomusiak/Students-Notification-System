namespace Domain.CourseSubChapters;

public record CourseSubChapterId(Guid Value)
{
    public static CourseSubChapterId Empty() => new(Guid.Empty);
    public static CourseSubChapterId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}