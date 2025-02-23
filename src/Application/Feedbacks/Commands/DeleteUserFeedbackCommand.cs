using System.Security.Claims;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Feedbacks.Exceptions;
using Application.Users.Exceptions;
using Domain.Feedbacks;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Feedbacks.Commands;

public record DeleteUserFeedbackCommand : IRequest<Either<FeedbackException, Feedback>>
{
    public required Guid FeedbackId { get; init; }
}

public class DeleteUserFeedbackCommandHandler(
    IHttpContextAccessor httpContextAccessor,
    IFeedbackRepository feedbackRepository,
    IFeedbackQueries feedbackQueries,
    UserManager<User> userManager) : IRequestHandler<DeleteUserFeedbackCommand, Either<FeedbackException, Feedback>>
{
    public async Task<Either<FeedbackException, Feedback>> Handle(DeleteUserFeedbackCommand request,
        CancellationToken cancellationToken)
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

        var existingFeedback = await feedbackQueries.GetById(new FeedbackId(request.FeedbackId), cancellationToken);

        return await existingFeedback.Match(
            async f => await DeleteFeedback(f, sessionUser.Id, cancellationToken),
            () => Task.FromResult<Either<FeedbackException, Feedback>>(new FeedbackNotFoundException(request.FeedbackId)));
    }

    private async Task<Either<FeedbackException, Feedback>> DeleteFeedback(Feedback feedback, Guid sessionUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await feedbackRepository.Delete(feedback, cancellationToken);
        }
        catch (Exception ex)
        {
            return new FeedbackUnknownException(sessionUserId, ex);
        }
    }

}