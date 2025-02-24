using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Courses.Exceptions;
using Domain.Courses;
using LanguageExt;
using MediatR;

namespace Application.Courses.Commands;

public record DeleteCourseCommand : IRequest<Either<CourseException, Course>>
{
    public required Guid CourseId { get; init; }
}

public class DeleteCourseCommandHandler(
    ICourseRepository courseRepository,
    ICourseQueries courseQueries,
    IFileStorageService fileStorageService) : IRequestHandler<DeleteCourseCommand, Either<CourseException, Course>>
{
    public async Task<Either<CourseException, Course>> Handle(
        DeleteCourseCommand request,
        CancellationToken cancellationToken)
    {
        var courseId = new CourseId(request.CourseId);
        var existingCourse = await courseQueries.GetById(courseId, cancellationToken);

        return await existingCourse.Match(
            async e => await DeleteEntity(e, cancellationToken), 
            () => Task.FromResult<Either<CourseException, Course>>(new CourseNotFoundException(courseId)));
    }

    private async Task<Either<CourseException, Course>> DeleteEntity(
        Course entity,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =  await courseRepository.Delete(entity, cancellationToken);
            
            if (!string.IsNullOrEmpty(entity.ImageUrl))
            {
                await fileStorageService.DeleteFileAsync("courses", entity.Id.Value, cancellationToken);   
            }
            
            return result;
        }
        catch (Exception exception)
        {
            return new CourseUnknownException(CourseId.Empty(), exception);
        }
    }
}