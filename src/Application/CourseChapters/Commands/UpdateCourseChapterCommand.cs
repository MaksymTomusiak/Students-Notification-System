using System.Runtime.InteropServices.Marshalling;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseChapters.Exceptions;
using Domain.CourseChapters;
using Domain.Courses;
using LanguageExt;
using MediatR;

namespace Application.CourseChapters.Commands;

public record UpdateCourseChapterCommand : IRequest<Either<CourseChapterException, CourseChapter>>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required uint EstimatedLearningTimeMinutes { get; init; }
}

public class UpdateCourseChapterCommandHandler(
    ICourseChapterRepository courseChapterRepository,
    ICourseChapterQueries courseChapterQueries) : IRequestHandler<UpdateCourseChapterCommand, Either<CourseChapterException, CourseChapter>>
{
    public async Task<Either<CourseChapterException, CourseChapter>> Handle(UpdateCourseChapterCommand request, CancellationToken cancellationToken)
    {
        var courseChapterId = new CourseChapterId(request.Id);
        
        var existingChapter = await courseChapterQueries.GetById(courseChapterId, cancellationToken);

        return await existingChapter.Match(
            async ec =>
            {
                var existingChapterName = await CheckDuplicated(courseChapterId, ec.CourseId, request.Name, cancellationToken);
                
                return await existingChapterName.Match(
                    ech => Task.FromResult<Either<CourseChapterException, CourseChapter>>(new CourseChapterAlreadyExistsException(ech.Id)),
                    async () => await UpdateEntity(ec, request.Name, request.EstimatedLearningTimeMinutes,cancellationToken));
            },
            () => Task.FromResult<Either<CourseChapterException, CourseChapter>>(new CourseChapterNotFoundException(courseChapterId)));
    }

    private async Task<Option<CourseChapter>> CheckDuplicated(CourseChapterId courseChapterId, CourseId courseId, string name, CancellationToken cancellationToken)
    {
        var chapter = await courseChapterQueries.SearchByNameAndCourse(name, courseId, cancellationToken);

        return chapter.Match(
            c => c.Id == courseChapterId ? Option<CourseChapter>.None : Option<CourseChapter>.Some(c),
            Option<CourseChapter>.None);
    }

    private async Task<Either<CourseChapterException, CourseChapter>> UpdateEntity(
        CourseChapter ec, 
        string name,
        uint estimatedLearningTimeMinutes,
        CancellationToken cancellationToken)
    {
        try
        {
            ec.UpdateDetails(name, estimatedLearningTimeMinutes, ec.Number);
            
            await courseChapterRepository.Update(ec, cancellationToken);
            
            var result = await courseChapterQueries.GetById(ec.Id, cancellationToken);
            
            return result.Match<Either<CourseChapterException, CourseChapter>>(
                c => c,
                () => new CourseChapterNotFoundException(ec.Id));
        }
        catch (Exception ex)
        {
            return new CourseChapterUnknownException(ec.Id, ex);
        }
    }
}