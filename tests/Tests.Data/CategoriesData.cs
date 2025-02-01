using Domain.Categories;

namespace Tests.Data;

public static class CategoriesData
{
    public static Category MainCategory =>
        Category.New(CategoryId.New(), "Main category name");
    public static Category SecondaryCategory =>
        Category.New(CategoryId.New(), "Secondary category name");
}