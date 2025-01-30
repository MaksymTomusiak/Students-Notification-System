using Domain.Categories;

namespace Api.Dtos;

public record CategoryDto(
    Guid Id,
    string Name)
{
    public static CategoryDto FromDomainModel(Category category)
        => new(category.Id.Value, category.Name);
}

public record CategoryCreateDto(
    string Name)
{
}