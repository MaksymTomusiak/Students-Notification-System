using Application.CourseChapters.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class CourseChapterErrorHandler
{
    public static ObjectResult ToObjectResult(this CourseChapterException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                CourseChapterNotFoundException or
                    CourseChapterCourseNotFoundException => StatusCodes.Status404NotFound,
                CourseChapterAlreadyExistsException  => StatusCodes.Status409Conflict,
                CourseChapterUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("CourseChapter error handler is not implemented")
            }
        };
    }
}