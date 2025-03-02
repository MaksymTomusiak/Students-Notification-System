using Application.Users.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class UserErrorHandler
{
    public static ObjectResult ToObjectResult(this UserException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                UserNotFoundException 
                or UserIdNotFoundException
                or RegisterCourseNotFoundException
                or UserFeedbackNotFoundException  => StatusCodes.Status404NotFound,
                InvalidCredentialsException => StatusCodes.Status401Unauthorized,
                UserWithNameAlreadyExistsException 
                or UserWithEmailAlreadyExistsException
                or RegisteredAlreadyFinishedException
                or UserAlreadyRegisteredException
                or UserNotRegisteredException
                or UserAlreadyLeftFeedbackException
                or UserBannedException
                or EmailNotVerifiedException
                or InvalidVerificationTokenException
                or EmailVerificationTokenExpiredException => StatusCodes.Status409Conflict,
                UserUnknownException => StatusCodes.Status500InternalServerError,
                UserUnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => throw new NotImplementedException("User error handler is not implemented")
            }
        };
    }
}