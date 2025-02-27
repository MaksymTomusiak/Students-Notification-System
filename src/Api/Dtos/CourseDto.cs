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
    IEnumerable<CategoryDto> Categories,
    IEnumerable<FeedbackDto> Feedbacks,
    IEnumerable<CourseChapterDto> Chapters)
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
                .Select(x => CategoryDto.FromDomainModel(x.Category!)),
            course.Feedbacks
                .Select(FeedbackDto.FromDomainModel),
            course.Chapters
                .Select(CourseChapterDto.FromDomainModel));
}

public record CourseCreateDto(
    string Name,
    IFormFile? Image,
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
    IFormFile? Image,
    string Description,
    DateTime StartDate,
    DateTime FinishDate,
    string Language,
    string Requirements,
    IEnumerable<Guid>? CategoriesIds);