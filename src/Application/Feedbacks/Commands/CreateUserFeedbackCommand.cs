using System.Security.Claims;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Feedbacks.Exceptions;
using Application.Users.Exceptions;
using Domain.Courses;
using Domain.Feedbacks;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Feedbacks.Commands;

public record CreateUserFeedbackCommand : IRequest<Either<FeedbackException, Feedback>>
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
    IRegisterQueries registerQueries) : IRequestHandler<CreateUserFeedbackCommand, Either<FeedbackException, Feedback>>
{
    public async Task<Either<FeedbackException, Feedback>> Handle(CreateUserFeedbackCommand request, CancellationToken cancellationToken)
    {
        var sessionUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sessionUserId))
        {
            return new FeedbackUserIdNotFoundException();
        }

        var sessionUser = await userManager.FindByIdAsync(sessionUserId);
        if (sessionUser == null)
        {
            return new FeedbackUserNotFoundException();
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
                            f => Task.FromResult<Either<FeedbackException, Feedback>>(
                                new FeedbackAlreadyExistsException(f.Id.Value)),
                            async () => await CreateFeedback(
                                courseId,
                                sessionUser.Id,
                                request.Rating,
                                request.Content,
                                cancellationToken));
                    },
                    () => Task.FromResult<Either<FeedbackException, Feedback>>(
                        new FeedbackWithoutRegistrationException(sessionUser.Id)));
            },
            () => Task.FromResult<Either<FeedbackException, Feedback>>(new FeedbackCourseNotFoundException()));
    }

    private async Task<Either<FeedbackException, Feedback>> CreateFeedback(CourseId courseId, Guid sessionUserId, ushort requestRating, string requestContent, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Feedback.New(FeedbackId.New(), courseId, sessionUserId,  DateTime.Now, requestContent, requestRating);
            
            return await feedbackRepository.Add(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            return new FeedbackUnknownException(Guid.Empty, ex);
        }
    }
}