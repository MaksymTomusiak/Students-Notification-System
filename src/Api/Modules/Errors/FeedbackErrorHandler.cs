using Application.Feedbacks.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class FeedbackErrorHandler
{
    public static ObjectResult ToObjectResult(this FeedbackException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                FeedbackNotFoundException
                    or FeedbackUserNotFoundException 
                    or FeedbackUserIdNotFoundException
                    or FeedbackCourseNotFoundException => StatusCodes.Status404NotFound,
                FeedbackAlreadyExistsException
                    or FeedbackWithoutRegistrationException => StatusCodes.Status409Conflict,
                FeedbackUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Feedback error handler is not implemented")
            }
        };
    }
}