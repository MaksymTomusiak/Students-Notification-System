using Domain.CourseCategories;

namespace Domain.Categories;

public class Category
{
    public CategoryId Id { get; }
    public string Name { get; private set; }
    public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    private Category(CategoryId id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public static Category New(CategoryId id, string name)
        => new(id, name);

    public void UpdateDetails(string name)
    {
        Name = name;
    }
}