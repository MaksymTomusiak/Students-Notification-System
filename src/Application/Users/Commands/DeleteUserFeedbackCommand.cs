﻿using System.Security.Claims;
using System.Text.Unicode;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Domain.Courses;
using Domain.Feedbacks;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands;

public record DeleteUserFeedbackCommand : IRequest<Either<UserException, Feedback>>
{
    public required Guid FeedbackId { get; init; }
}

public class DeleteUserFeedbackCommandHandler(
    IHttpContextAccessor httpContextAccessor,
    IFeedbackRepository feedbackRepository,
    IFeedbackQueries feedbackQueries,
    UserManager<User> userManager) : IRequestHandler<DeleteUserFeedbackCommand, Either<UserException, Feedback>>
{
    public async Task<Either<UserException, Feedback>> Handle(DeleteUserFeedbackCommand request,
        CancellationToken cancellationToken)
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

        var existingFeedback = await feedbackQueries.GetById(new FeedbackId(request.FeedbackId), cancellationToken);

        return await existingFeedback.Match(
            async f => await DeleteFeedback(f, sessionUser.Id, cancellationToken),
            () => Task.FromResult<Either<UserException, Feedback>>(new UserFeedbackNotFoundException()));
    }

    private async Task<Either<UserException, Feedback>> DeleteFeedback(Feedback feedback, Guid sessionUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await feedbackRepository.Delete(feedback, cancellationToken);
        }
        catch (Exception ex)
        {
            return new UserUnknownException(sessionUserId, ex);
        }
    }

}