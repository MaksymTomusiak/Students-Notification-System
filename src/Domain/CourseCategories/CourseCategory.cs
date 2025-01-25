using Domain.Categories;
using Domain.Courses;

namespace Domain.CourseCategories;

public class CourseCategory
{
    public CourseCategoryId Id { get; }
    public CourseId CourseId { get; }
    public Course? Course { get; }
    public CategoryId CategoryId { get;}
    public Category? Category { get; }

    private CourseCategory(CourseCategoryId id, CourseId courseId, CategoryId categoryId)
    {
        Id = id;
        CourseId = courseId;
        CategoryId = categoryId;
    }
    
    public static CourseCategory New(CourseCategoryId id, CourseId courseId, CategoryId categoryId)
        => new(id, courseId, categoryId);
}