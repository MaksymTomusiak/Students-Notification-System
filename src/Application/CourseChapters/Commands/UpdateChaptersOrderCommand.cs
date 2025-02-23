using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseChapters.Exceptions;
using Domain.CourseChapters;
using LanguageExt;
using MediatR;

namespace Application.CourseChapters.Commands;

public record UpdateChaptersOrderCommand : IRequest<Either<CourseChapterException, bool>>
{
    public required IReadOnlyList<Guid> ChaptersIds { get; init; }
    public required IReadOnlyList<uint> Numbers { get; init; }
}

public class UpdateChaptersOrderCommandHandler(
    ICourseChapterQueries courseChapterQueries,
    ICourseChapterRepository courseChapterRepository) : IRequestHandler<UpdateChaptersOrderCommand, Either<CourseChapterException, bool>>
{
    public async Task<Either<CourseChapterException, bool>> Handle(UpdateChaptersOrderCommand request, CancellationToken cancellationToken)
    {
        List<CourseChapter> chapters = [];
        foreach (var id in request.ChaptersIds)
        {
            var chapterId = new CourseChapterId(id);
            var existingChapter = await courseChapterQueries.GetById(chapterId, cancellationToken);
            
            var exists = existingChapter.Match(
                ec =>
                {
                    chapters.Add(ec);
                    return true;
                },
                () =>  false);
            
            if (!exists)
            {
                return new CourseChapterNotFoundException(chapterId);
            }
        }
        
        return await UpdateOrders(chapters, request.Numbers, cancellationToken);
    }

    private async Task<Either<CourseChapterException, bool>> UpdateOrders(List<CourseChapter> chapters, IReadOnlyList<uint> requestNumbers, CancellationToken cancellationToken)
    {
        try
        {
            for (var i = 0; i < chapters.Count; i++)
            {
                chapters[i].UpdateNumber(requestNumbers[i]);
                
                await courseChapterRepository.Update(chapters[i], cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            return new CourseChapterUnknownException(CourseChapterId.Empty(), ex);
        }
    }
}