using System.Security.Claims;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Courses;
using Domain.Registers;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record EnrollUserInCourseCommand : IRequest<Either<UserException, Register>>
{
    public required Guid CourseId { get; init; }
}

public class EnrollUserInCourseCommandHandler(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    ICourseQueries courseQueries,
    IRegisterQueries registerQueries,
    IRegisterRepository registerRepository) : IRequestHandler<EnrollUserInCourseCommand, Either<UserException, Register>>
{
    public async Task<Either<UserException, Register>> Handle(EnrollUserInCourseCommand request, CancellationToken cancellationToken)
    {
        var sessionUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sessionUserId))
        {
            return new UserIdNotFoundException();
        }

        var sessionUser = await userManager.FindByIdAsync(sessionUserId);
        if (sessionUser == null)
        {
            return new UserNotFoundException(new Guid(sessionUserId));
        }

        var courseId = new CourseId(request.CourseId);

        var existingCourse = await courseQueries.GetById(courseId, cancellationToken);
        
        return await existingCourse.Match(
            async ec =>
            {
                var existingRegister =
                    await registerQueries.GetByCourseAndUser(courseId, sessionUser.Id, cancellationToken);

                return await existingRegister.Match(
                    r => Task.FromResult<Either<UserException, Register>>(
                        new UserAlreadyRegisteredException(sessionUser.Id)),
                    async () =>
                    {
                        if (ec.FinishDate < DateTime.Now)
                        {
                            return new RegisteredAlreadyFinishedException();
                        }
                        return await EnrollUser(courseId, sessionUser.Id, cancellationToken);
                    });
                
            },
            () => Task.FromResult<Either<UserException, Register>>(new RegisteredCourseNotFoundException()));
    }

    private async Task<Either<UserException, Register>> EnrollUser(CourseId courseId, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Register.New(RegisterId.New(), DateTime.Now, courseId, userId);
            
            var res = await registerRepository.Add(entity, cancellationToken);

            return res;
        }
        catch (Exception ex)
        {
            return new UserUnknownException(userId, ex);
        }
    }
}
