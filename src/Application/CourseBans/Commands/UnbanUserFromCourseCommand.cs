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

public record UnbanUserFromCourseCommand : IRequest<Either<CourseBanException, CourseBan>>
{
    public required Guid CourseId { get; init; }
    public required Guid UserId { get; init; }
}

public class UnbanUserFromCourseCommandHandler(
    ICourseQueries courseQueries,
    UserManager<User> userManager,
    ICourseBanQueries courseBanQueries,
    ICourseBanRepository courseBanRepository) : IRequestHandler<UnbanUserFromCourseCommand, Either<CourseBanException, CourseBan>>
{
    public async Task<Either<CourseBanException, CourseBan>> Handle(UnbanUserFromCourseCommand request, CancellationToken cancellationToken)
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
                    async cb => await DeleteBan(cb, cancellationToken),
                    () => Task.FromResult<Either<CourseBanException, CourseBan>>(
                        new BanNotFoundException()));
            },
            () => Task.FromResult<Either<CourseBanException, CourseBan>>(new BanCourseNotFoundException()));
    }

    private async Task<Either<CourseBanException, CourseBan>> DeleteBan(CourseBan courseBan, CancellationToken cancellationToken)
    {
        try
        {
            var res = await courseBanRepository.Delete(courseBan, cancellationToken);

            return res;
        }
        catch (Exception ex)
        {
            return new BanUnknownException(courseBan.UserId, ex);
        }
    }
}