
using System.Security.Claims;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Courses;
using Domain.Feedbacks;
using Domain.Registers;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record CreateUserFeedbackCommand : IRequest<Either<UserException, Feedback>>
{
    public required Guid CourseId { get; init; }
    public required ushort Rating { get; init; }
    public required string Content { get; init; }
}

public class CreateUserFeedbackCommandHandler(
    IHttpContextAccessor httpContextAccessor,
    IFeedbackRepository feedbackRepository,
    IFeedbackQueries feedbackQueries,
    ICourseQueries courseQueries,
    UserManager<User> userManager,
    IRegisterQueries registerQueries) : IRequestHandler<CreateUserFeedbackCommand, Either<UserException, Feedback>>
{
    public async Task<Either<UserException, Feedback>> Handle(CreateUserFeedbackCommand request, CancellationToken cancellationToken)
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
                var existingRegister = await registerQueries.GetByCourseAndUser(courseId, sessionUser.Id, cancellationToken);

                return await existingRegister.Match(
                    async r =>
                    {
                        var existingFeedback = await feedbackQueries.GetByCourseAndUser(courseId, sessionUser.Id, cancellationToken);

                        return await existingFeedback.Match(
                            f => Task.FromResult<Either<UserException, Feedback>>(
                                new UserAlreadyLeftFeedbackException(sessionUser.Id)),
                            async () => await CreateFeedback(
                                courseId,
                                sessionUser.Id,
                                request.Rating,
                                request.Content,
                                cancellationToken));
                    },
                    () => Task.FromResult<Either<UserException, Feedback>>(
                        new UserNotRegisteredException(sessionUser.Id)));
            },
            () => Task.FromResult<Either<UserException, Feedback>>(new RegisterCourseNotFoundException()));
    }

    private async Task<Either<UserException, Feedback>> CreateFeedback(CourseId courseId, Guid sessionUserId, ushort requestRating, string requestContent, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Feedback.New(FeedbackId.New(), courseId, sessionUserId,  DateTime.Now, requestContent, requestRating);
            
            return await feedbackRepository.Add(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            return new UserUnknownException(sessionUserId, ex);
        }
    }
}