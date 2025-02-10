namespace Application.CourseBans.Exceptions;

public class CourseBanException(Guid id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public Guid Id { get; } = id;
}

public class BanCourseNotFoundException() 
    : CourseBanException(Guid.Empty, $"Course to ban user from was not found!");

public class BanUserNotFoundException() 
    : CourseBanException(Guid.Empty, $"User to ban was not found!");

public class BanNotFoundException() 
    : CourseBanException(Guid.Empty, $"Could not find ban!");

public class BanAlreadyExists(Guid id) 
    : CourseBanException(id, $"Such ban already exists under id:{id}!");

public class BanUnknownException(Guid id, Exception innerException)
    : CourseBanException(id, $"Unknown exception for the ban under id: {id}!", innerException);