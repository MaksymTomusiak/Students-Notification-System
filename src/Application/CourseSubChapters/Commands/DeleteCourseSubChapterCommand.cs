using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseSubChapters.Exceptions;
using Domain.CourseSubChapters;
using LanguageExt;
using MediatR;

namespace Application.CourseSubChapters.Commands;

public record DeleteCourseSubChapterCommand : IRequest<Either<CourseSubChapterException, CourseSubChapter>>
{
    public required Guid Id { get; init; }
}

public class DeleteCourseSubChapterCommandHandler(
    ICourseSubChapterQueries courseSubChapterQueries,
    ICourseSubChapterRepository courseSubChapterRepository) 
    : IRequestHandler<DeleteCourseSubChapterCommand, Either<CourseSubChapterException, CourseSubChapter>>
{
    public async Task<Either<CourseSubChapterException, CourseSubChapter>> Handle(DeleteCourseSubChapterCommand request, CancellationToken cancellationToken)
    {
        var courseSubChapterId = new CourseSubChapterId(request.Id);
        
        var existingSubChapter = await courseSubChapterQueries.GetById(courseSubChapterId, cancellationToken);

        return await existingSubChapter.Match(
            async esc => await DeleteAndReorderSubChapters(esc, cancellationToken),
            () => Task.FromResult<Either<CourseSubChapterException, CourseSubChapter>>(new CourseSubChapterNotFoundException(courseSubChapterId)));
    }

    private async Task<Either<CourseSubChapterException, CourseSubChapter>> DeleteAndReorderSubChapters(CourseSubChapter subChapterToDelete, CancellationToken cancellationToken)
    {
        try
        {
            // Delete the subchapter
            var deleteResult = await courseSubChapterRepository.Delete(subChapterToDelete, cancellationToken);

            // Get all subchapters of the same chapter that have a higher number
            var remainingSubChapters = await courseSubChapterQueries.GetByCourseChapterId(subChapterToDelete.CourseChapterId, cancellationToken);
            var affectedSubChapters = remainingSubChapters.Where(sc => sc.Number > subChapterToDelete.Number).ToList();

            // Update numbers
            foreach (var subChapter in affectedSubChapters)
            {
                subChapter.UpdateNumber(subChapter.Number - 1);
                await courseSubChapterRepository.Update(subChapter, cancellationToken);
            }

            return deleteResult;
        }
        catch (Exception ex)
        {
            return new CourseSubChapterUnknownException(subChapterToDelete.Id, ex);
        }
    }
}
