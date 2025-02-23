using Domain.CourseChapters;

namespace Api.Dtos;

public record CourseChapterDto(
    Guid Id,
    Guid CourseId,
    string Name,
    uint EstimatedTime,
    uint Number,
    IList<CourseSubChapterDto>? SubChapters = null)
{
    public static CourseChapterDto FromDomainModel(CourseChapter chapter) 
        => new(chapter.Id.Value,
            chapter.CourseId.Value,
            chapter.Name,
            chapter.EstimatedLearningTimeMinutes,
            chapter.Number,
            chapter.SubChapters.Select(CourseSubChapterDto.FromDomainModel).ToList());
}

public record CourseChapterCreateDto(
    Guid CourseId,
    string Name,
    uint EstimatedTime);
    
public record CourseChapterUpdateDto(
    Guid Id,
    string Name,
    uint EstimatedTime);
    
public record CourseChaptersUpdateOrderDto(
    IEnumerable<Guid> ChaptersIds,
    IEnumerable<uint> Numbers);