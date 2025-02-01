using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;

namespace Tests.Data;

public static class CourseCategoriesData
{
    public static CourseCategory New(CourseId courseId, CategoryId categoryId)
        => CourseCategory.New(CourseCategoryId.New(), courseId, categoryId);
}