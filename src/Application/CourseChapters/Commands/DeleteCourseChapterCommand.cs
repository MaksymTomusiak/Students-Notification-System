using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseChapters.Exceptions;
using Domain.CourseChapters;
using LanguageExt;
using MediatR;

namespace Application.CourseChapters.Commands;

public record DeleteCourseChapterCommand : IRequest<Either<CourseChapterException, CourseChapter>>
{
    public required Guid Id { get; init; }
}

public class DeleteCourseChapterCommandHandler(
    ICourseChapterQueries courseChapterQueries,
    ICourseChapterRepository courseChapterRepository) 
    : IRequestHandler<DeleteCourseChapterCommand, Either<CourseChapterException, CourseChapter>>
{
    public async Task<Either<CourseChapterException, CourseChapter>> Handle(DeleteCourseChapterCommand request, CancellationToken cancellationToken)
    {
        var courseChapterId = new CourseChapterId(request.Id);
        
        var existingChapter = await courseChapterQueries.GetById(courseChapterId, cancellationToken);

        return await existingChapter.Match(
            async ec => await DeleteAndReorderChapters(ec, cancellationToken),
            () => Task.FromResult<Either<CourseChapterException, CourseChapter>>(new CourseChapterNotFoundException(courseChapterId)));
    }

    private async Task<Either<CourseChapterException, CourseChapter>> DeleteAndReorderChapters(CourseChapter chapterToDelete, CancellationToken cancellationToken)
    {
        try
        {
            // Delete the chapter
            var deleteResult = await courseChapterRepository.Delete(chapterToDelete, cancellationToken);

            // Get all chapters of the same course that have a higher number
            var remainingChapters = await courseChapterQueries.GetByCourseId(chapterToDelete.CourseId, cancellationToken);
            var affectedChapters = remainingChapters.Where(c => c.Number > chapterToDelete.Number).ToList();

            // Update numbers
            foreach (var chapter in affectedChapters)
            {
                chapter.UpdateNumber(chapter.Number - 1);
                await courseChapterRepository.Update(chapter, cancellationToken);
            }

            return deleteResult;
        }
        catch (Exception ex)
        {
            return new CourseChapterUnknownException(chapterToDelete.Id, ex);
        }
    }
}
