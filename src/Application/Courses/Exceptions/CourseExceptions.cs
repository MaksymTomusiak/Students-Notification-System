using Domain.Courses;

namespace Application.Courses.Exceptions;

public class CourseException(CourseId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public CourseId CourseId { get; } = id;
}

public class CourseNotFoundException(CourseId id) : CourseException(id, $"Course under id {id} not found!");

public class CourseCreatorNotFoundException(CourseId id) : CourseException(id, $"Course organizer not found!");

public class CourseCategoryNotFoundException(CourseId id) : CourseException(id, $"Course category not found!");

public class CourseAlreadyExistsException(CourseId id) : CourseException(id, $"Such course already exists!");

public class CourseUnknownException(CourseId id, Exception innerException)
    : CourseException(id, $"Unknown exception for the course under id {id}!", innerException);