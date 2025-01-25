namespace Domain.CourseCategories;

public record CourseCategoryId(Guid Value)
{
    public static CourseCategoryId Empty() => new(Guid.Empty);
    public static CourseCategoryId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}