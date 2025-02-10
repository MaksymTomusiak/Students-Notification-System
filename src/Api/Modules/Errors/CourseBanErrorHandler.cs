using Application.CourseBans.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class CourseBanErrorHandler
{
    public static ObjectResult ToObjectResult(this CourseBanException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                BanNotFoundException 
                or BanUserNotFoundException
                or BanCourseNotFoundException=> StatusCodes.Status404NotFound,
                BanAlreadyExists => StatusCodes.Status409Conflict,
                BanUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("User error handler is not implemented")
            }
        };
    }
}