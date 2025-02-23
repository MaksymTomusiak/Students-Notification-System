using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseChapters.Exceptions;
using Domain.CourseChapters;
using Domain.Courses;
using LanguageExt;
using MediatR;

namespace Application.CourseChapters.Commands;

public record CreateCourseChapterCommand : IRequest<Either<CourseChapterException, CourseChapter>>
{
    public required Guid CourseId { get; init; }
    public required string Name { get; init; }
    public required uint EstimatedLearningTimeMinutes { get; init; }
}

public class CreateCourseChapterCommandHandler(
    ICourseQueries courseQueries,
    ICourseChapterRepository courseChapterRepository,
    ICourseChapterQueries courseChapterQueries)
    : IRequestHandler<CreateCourseChapterCommand, Either<CourseChapterException, CourseChapter>>
{
    public async Task<Either<CourseChapterException, CourseChapter>> Handle(
        CreateCourseChapterCommand request,
        CancellationToken cancellationToken)
    {
        var existingCourse = await courseQueries.GetById(new CourseId(request.CourseId), cancellationToken);

        return await existingCourse.Match(
            async ec =>
            {
                var existingChapter = await courseChapterQueries.SearchByNameAndCourse(request.Name, ec.Id, cancellationToken);

                return await existingChapter.Match(
                    ech => Task.FromResult<Either<CourseChapterException, CourseChapter>>(
                        new CourseChapterAlreadyExistsException(ech.Id)),
                    async () =>
                    {
                        var chapters = await courseChapterQueries.GetByCourseId(ec.Id, cancellationToken);
                        uint nextChapterNumber = 1;
                        if (chapters.Count != 0)
                        {
                            var lastChapterNumber = chapters.Max(x => x.Number);
                            nextChapterNumber += lastChapterNumber;
                        }

                        var newChapter = CourseChapter.New(
                            CourseChapterId.New(),
                            new CourseId(request.CourseId),
                            request.Name,
                            request.EstimatedLearningTimeMinutes,
                            nextChapterNumber
                        );

                        return await CreateEntity(newChapter, cancellationToken);
                    }
                );
            },
            () => Task.FromResult<Either<CourseChapterException, CourseChapter>>(new CourseChapterCourseNotFoundException(CourseChapterId.New())));
    }

    private async Task<Either<CourseChapterException, CourseChapter>> CreateEntity(CourseChapter newChapter, CancellationToken cancellationToken)
    {
        try
        {
            return await courseChapterRepository.Add(newChapter, cancellationToken);
        }
        catch (Exception ex)
        {
            return new CourseChapterUnknownException(CourseChapterId.Empty(), ex);
        }
    }
}
