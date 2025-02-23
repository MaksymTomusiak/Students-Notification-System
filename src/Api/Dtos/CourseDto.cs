using Domain.Courses;

namespace Api.Dtos;

public record CourseDto(
    Guid Id,
    string Name,
    string ImageUrl,
    string Description,
    Guid CreatorId,
    DateTime StartDate,
    DateTime FinishDate,
    string Language,
    string Requirements,
    IEnumerable<CategoryDto> Categories)
{
    public static CourseDto FromDomainModel(Course course)
        => new(course.Id.Value,
            course.Name,
            course.ImageUrl,
            course.Description,
            course.CreatorId,
            course.StartDate,
            course.FinishDate,
            course.Language,
            course.Requirements,
            course.CourseCategories.Where(x => x.Category != null)
                .Select(x => CategoryDto.FromDomainModel(x.Category!)));
}

public record CourseCreateDto(
    string Name,
    string ImageUrl,
    string Description,
    Guid CreatorId,
    DateTime StartDate,
    DateTime FinishDate,
    string Language,
    string Requirements,
    IEnumerable<Guid> CategoriesIds);
    
public record CourseUpdateDto(
    Guid Id,
    string Name,
    string ImageUrl,
    string Description,
    DateTime StartDate,
    DateTime FinishDate,
    string Language,
    string Requirements,
    IEnumerable<Guid>? CategoriesIds);