using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseSubChapters.Exceptions;
using Domain.CourseSubChapters;
using LanguageExt;
using MediatR;

namespace Application.CourseSubChapters.Commands;

public record UpdateCourseSubChaptersOrderCommand : IRequest<Either<CourseSubChapterException, bool>>
{
    public required IReadOnlyList<Guid> ChaptersIds { get; init; }
    public required IReadOnlyList<uint> Numbers { get; init; }
}

public class UpdateCourseSubChaptersOrderCommandHandler(
    ICourseSubChapterQueries courseSubChapterQueries,
    ICourseSubChapterRepository courseSubChapterRepository) : IRequestHandler<UpdateCourseSubChaptersOrderCommand, Either<CourseSubChapterException, bool>>
{
    public async Task<Either<CourseSubChapterException, bool>> Handle(UpdateCourseSubChaptersOrderCommand request, CancellationToken cancellationToken)
    {
        List<CourseSubChapter> subChapters = [];
        foreach (var id in request.ChaptersIds)
        {
            var subChapterId = new CourseSubChapterId(id);
            var existingSubChapter = await courseSubChapterQueries.GetById(subChapterId, cancellationToken);
            
            var exists = existingSubChapter.Match(
                ec =>
                {
                    subChapters.Add(ec);
                    return true;
                },
                () =>  false);
            
            if (!exists)
            {
                return new CourseSubChapterNotFoundException(subChapterId);
            }
        }
        
        return await UpdateOrders(subChapters, request.Numbers, cancellationToken);
    }

    private async Task<Either<CourseSubChapterException, bool>> UpdateOrders(List<CourseSubChapter> subChapters, IReadOnlyList<uint> requestNumbers, CancellationToken cancellationToken)
    {
        try
        {
            for (var i = 0; i < subChapters.Count; i++)
            {
                subChapters[i].UpdateNumber(requestNumbers[i]);
                
                await courseSubChapterRepository.Update(subChapters[i], cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            return new CourseSubChapterUnknownException(CourseSubChapterId.Empty(), ex);
        }
    }
}