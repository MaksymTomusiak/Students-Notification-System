using Domain.CourseSubChapters;

namespace Application.CourseSubChapters.Exceptions;

public class CourseSubChapterException(CourseSubChapterId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public CourseSubChapterId CourseSubChapterId { get; } = id;
}

public class CourseSubChapterNotFoundException(CourseSubChapterId id) : CourseSubChapterException(id, $"CourseSubChapter under id {id} not found!");

public class CourseSubChapterAlreadyExistsException(CourseSubChapterId id) : CourseSubChapterException(id, $"Such courseSubChapter already exists!");

public class CourseSubChapterChapterNotFoundException(CourseSubChapterId id) : CourseSubChapterException(id, $"Chapter of this subchapter was not found!");

public class CourseSubChapterUnknownException(CourseSubChapterId id, Exception innerException)
    : CourseSubChapterException(id, $"Unknown exception for the courseSubChapter under id {id}!", innerException);