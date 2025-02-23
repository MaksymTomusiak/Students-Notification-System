using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseSubChapters.Exceptions;
using Domain.CourseChapters;
using Domain.CourseSubChapters;
using LanguageExt;
using MediatR;

namespace Application.CourseSubChapters.Commands;

public record CreateCourseSubChapterCommand : IRequest<Either<CourseSubChapterException, CourseSubChapter>>
{
    public required Guid CourseChapterId { get; init; }
    public required string Name { get; init; }
    public required string Content { get; init; }
    public required uint EstimatedLearningTimeMinutes { get; init; }
}

public class CreateCourseSubChapterCommandHandler(
    ICourseChapterQueries courseChapterQueries,
    ICourseSubChapterRepository courseSubChapterRepository,
    ICourseSubChapterQueries courseSubChapterQueries)
    : IRequestHandler<CreateCourseSubChapterCommand, Either<CourseSubChapterException, CourseSubChapter>>
{
    public async Task<Either<CourseSubChapterException, CourseSubChapter>> Handle(
        CreateCourseSubChapterCommand request,
        CancellationToken cancellationToken)
    {
        var existingChapter = await courseChapterQueries.GetById(new CourseChapterId(request.CourseChapterId), cancellationToken);

        return await existingChapter.Match(
            async ec =>
            {
                var existingSubChapter = await courseSubChapterQueries.SearchByNameAndChapter(request.Name, ec.Id, cancellationToken);

                return await existingSubChapter.Match(
                    esc => Task.FromResult<Either<CourseSubChapterException, CourseSubChapter>>(
                        new CourseSubChapterAlreadyExistsException(esc.Id)),
                    async () =>
                    {
                        var subChapters = await courseSubChapterQueries.GetByCourseChapterId(ec.Id, cancellationToken);
                        uint nextSubChapterNumber = 1;
                        if (subChapters.Count != 0)
                        {
                            var lastSubChapterNumber = subChapters.Max(x => x.Number);
                            nextSubChapterNumber += lastSubChapterNumber;
                        }

                        var newSubChapter = CourseSubChapter.New(
                            CourseSubChapterId.New(),
                            new CourseChapterId(request.CourseChapterId),
                            request.Name,
                            request.Content,
                            request.EstimatedLearningTimeMinutes,
                            nextSubChapterNumber
                        );

                        return await CreateEntity(newSubChapter, cancellationToken);
                    }
                );
            },
            () => Task.FromResult<Either<CourseSubChapterException, CourseSubChapter>>(
                new CourseSubChapterChapterNotFoundException(CourseSubChapterId.New()))
        );
    }

    private async Task<Either<CourseSubChapterException, CourseSubChapter>> CreateEntity(CourseSubChapter newSubChapter, CancellationToken cancellationToken)
    {
        try
        {
            return await courseSubChapterRepository.Add(newSubChapter, cancellationToken);
        }
        catch (Exception ex)
        {
            return new CourseSubChapterUnknownException(CourseSubChapterId.Empty(), ex);
        }
    }
}
