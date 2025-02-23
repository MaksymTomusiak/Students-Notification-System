using Domain.CourseChapters;

namespace Application.CourseChapters.Exceptions;

public class CourseChapterException(CourseChapterId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public CourseChapterId CourseChapterId { get; } = id;
}

public class CourseChapterNotFoundException(CourseChapterId id) : CourseChapterException(id, $"CourseChapter under id {id} not found!");

public class CourseChapterAlreadyExistsException(CourseChapterId id) : CourseChapterException(id, $"Such courseChapter already exists!");

public class CourseChapterCourseNotFoundException(CourseChapterId id) : CourseChapterException(id, $"Course of this chapter was not found!");

public class CourseChapterUnknownException(CourseChapterId id, Exception innerException)
    : CourseChapterException(id, $"Unknown exception for the courseChapter under id {id}!", innerException);