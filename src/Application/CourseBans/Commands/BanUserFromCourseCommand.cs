using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.CourseBans.Exceptions;
using Application.Users.Exceptions;
using Domain.CourseBans;
using Domain.Courses;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CourseBans.Commands;

public record BanUserFromCourseCommand : IRequest<Either<CourseBanException, CourseBan>>
{
    public required Guid CourseId { get; init; }
    public required Guid UserId { get; init; }
    public required string Reason { get; init; }
}

public class BanUserFromCourseCommandHandler(
    ICourseQueries courseQueries,
    UserManager<User> userManager,
    ICourseBanQueries courseBanQueries,
    ICourseBanRepository courseBanRepository) : IRequestHandler<BanUserFromCourseCommand, Either<CourseBanException, CourseBan>>
{
    public async Task<Either<CourseBanException, CourseBan>> Handle(BanUserFromCourseCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByIdAsync(request.UserId.ToString());

        if (existingUser == null)
        {
            return new BanUserNotFoundException();
        }

        var courseId = new CourseId(request.CourseId);

        var existingCourse = await courseQueries.GetById(courseId, cancellationToken);

        return await existingCourse.Match(
            async ec =>
            {
                var existingCourseBan = 
                    await courseBanQueries.GetByCourseAndUser(courseId, request.UserId, cancellationToken);

                return await existingCourseBan.Match(
                    cb => Task.FromResult<Either<CourseBanException, CourseBan>>(
                        new BanAlreadyExists(request.UserId)),
                    async () =>
                    {
                        return await BanUserFromCourse(courseId, request.UserId, request.Reason, cancellationToken);
                    });
            },
            () => Task.FromResult<Either<CourseBanException, CourseBan>>(new BanCourseNotFoundException()));
    }

    private async Task<Either<CourseBanException, CourseBan>> BanUserFromCourse(CourseId courseId, Guid userId, string reason, CancellationToken cancellationToken)
    {
        try
        {
            var entity = CourseBan.New(CourseBanId.New(), userId, courseId, reason, DateTime.Now);
            
            var res = await courseBanRepository.Add(entity, cancellationToken);

            return res;
        }
        catch (Exception ex)
        {
            return new BanUnknownException(userId, ex);
        }
    }
}