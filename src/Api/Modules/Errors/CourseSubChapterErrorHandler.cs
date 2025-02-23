using Application.CourseSubChapters.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class CourseSubChapterErrorHandler
{
    public static ObjectResult ToObjectResult(this CourseSubChapterException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                CourseSubChapterNotFoundException or
                    CourseSubChapterChapterNotFoundException => StatusCodes.Status404NotFound,
                CourseSubChapterAlreadyExistsException  => StatusCodes.Status409Conflict,
                CourseSubChapterUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("CourseSubChapter error handler is not implemented")
            }
        };
    }
}