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

public class UnregisterUserFromCourseCommand : IRequest<Either<UserException, Register>>
{
    public required Guid CourseId { get; init; }
}

public class UnregisterUserFromCourseCommandHandler(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    IRegisterRepository registerRepository,
    IRegisterQueries registerQueries,
    ICourseQueries courseQueries) : IRequestHandler<UnregisterUserFromCourseCommand, Either<UserException, Register>>
{
    public async Task<Either<UserException, Register>> Handle(UnregisterUserFromCourseCommand request, CancellationToken cancellationToken)
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
        
        var existingCourse = await courseQueries.GetById(new CourseId(request.CourseId), cancellationToken);
        
        return await existingCourse.Match(
            async ec =>
            {
                var existingRegister = await registerQueries.GetByCourseAndUser(new CourseId(request.CourseId), sessionUser.Id, cancellationToken);

                return await existingRegister.Match(
                    r => RemoveRegister(r, new Guid(sessionUserId), cancellationToken),
                    () => Task.FromResult<Either<UserException, Register>>(
                        new UserNotRegisteredException(sessionUser.Id)));
            },
            () => Task.FromResult<Either<UserException, Register>>(new RegisteredCourseNotFoundException()));
    }

    private async Task<Either<UserException, Register>> RemoveRegister(Register register, Guid sessionUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await registerRepository.Delete(register, cancellationToken);
        }
        catch (Exception ex)
        {
            return new UserUnknownException(sessionUserId, ex);
        }
    }
}