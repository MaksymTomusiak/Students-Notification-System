using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseSubChapters.Exceptions;
using Domain.CourseChapters;
using Domain.CourseSubChapters;
using LanguageExt;
using MediatR;

namespace Application.CourseSubChapters.Commands;

public record UpdateCourseSubChapterCommand : IRequest<Either<CourseSubChapterException, CourseSubChapter>>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Content { get; init; }
    public required uint EstimatedLearningTimeMinutes { get; init; }
}

public class UpdateCourseSubChapterCommandHandler(
    ICourseSubChapterRepository courseSubChapterRepository,
    ICourseSubChapterQueries courseSubChapterQueries) 
    : IRequestHandler<UpdateCourseSubChapterCommand, Either<CourseSubChapterException, CourseSubChapter>>
{
    public async Task<Either<CourseSubChapterException, CourseSubChapter>> Handle(UpdateCourseSubChapterCommand request, CancellationToken cancellationToken)
    {
        var courseSubChapterId = new CourseSubChapterId(request.Id);
        
        var existingSubChapter = await courseSubChapterQueries.GetById(courseSubChapterId, cancellationToken);

        return await existingSubChapter.Match(
            async esc =>
            {
                var existingSubChapterName = await CheckDuplicated(courseSubChapterId, esc.CourseChapterId, request.Name, cancellationToken);
                
                return await existingSubChapterName.Match(
                    escn => Task.FromResult<Either<CourseSubChapterException, CourseSubChapter>>(
                        new CourseSubChapterAlreadyExistsException(escn.Id)),
                    async () => await UpdateEntity(esc, request.Name, request.Content, request.EstimatedLearningTimeMinutes, cancellationToken));
            },
            () => Task.FromResult<Either<CourseSubChapterException, CourseSubChapter>>(
                new CourseSubChapterNotFoundException(courseSubChapterId)));
    }

    private async Task<Option<CourseSubChapter>> CheckDuplicated(CourseSubChapterId courseSubChapterId, CourseChapterId chapterId, string name, CancellationToken cancellationToken)
    {
        var subChapter = await courseSubChapterQueries.SearchByNameAndChapter(name, chapterId, cancellationToken);

        return subChapter.Match(
            sc => sc.Id == courseSubChapterId ? Option<CourseSubChapter>.None : Option<CourseSubChapter>.Some(sc),
            Option<CourseSubChapter>.None);
    }

    private async Task<Either<CourseSubChapterException, CourseSubChapter>> UpdateEntity(
        CourseSubChapter esc, 
        string name,
        string content,
        uint estimatedLearningTimeMinutes,
        CancellationToken cancellationToken)
    {
        try
        {
            esc.UpdateDetails(name, content, estimatedLearningTimeMinutes, esc.Number);
            
            return await courseSubChapterRepository.Update(esc, cancellationToken);
        }
        catch (Exception ex)
        {
            return new CourseSubChapterUnknownException(esc.Id, ex);
        }
    }
}
