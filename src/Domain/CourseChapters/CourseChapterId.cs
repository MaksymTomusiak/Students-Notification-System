namespace Domain.CourseChapters;

public record CourseChapterId(Guid Value)
{
    public static CourseChapterId Empty() => new(Guid.Empty);
    public static CourseChapterId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}