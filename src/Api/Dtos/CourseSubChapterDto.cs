using Domain.CourseSubChapters;

namespace Api.Dtos;

public record CourseSubChapterDto(
    Guid Id,
    Guid CourseChapterId,
    string Name,
    string Content,
    uint EstimateTime,
    uint Number)
{
    public static CourseSubChapterDto FromDomainModel(CourseSubChapter subChapter) =>
        new(subChapter.Id.Value,
            subChapter.CourseChapterId.Value,
            subChapter.Name,
            subChapter.Content,
            subChapter.EstimatedLearningTimeMinutes,
            subChapter.Number);
}

public record CourseSubChapterCreateDto(
    Guid ChapterId, 
    string Name,
    string Content, 
    uint EstimateTime);
    
public record CourseSubChapterUpdateDto(
    Guid Id,
    string Name,
    string Content, 
    uint EstimateTime);
    
public record CourseSubChaptersUpdateOrderDto(
    IEnumerable<Guid> SubChaptersIds,
    IEnumerable<uint> Numbers);