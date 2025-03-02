namespace Application.Users.Exceptions;

public class UserException(Guid id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public Guid Id { get; } = id;
}

public class UserNotFoundException(Guid id) 
    : UserException(id, $"User under id: {id} not found!");

public class UserIdNotFoundException() 
    : UserException(Guid.Empty, $"User id not found!");

public class UserFeedbackNotFoundException() 
    : UserException(Guid.Empty, $"User feedback not found!");

public class RegisterCourseNotFoundException() 
    : UserException(Guid.Empty, $"Course to register not found!");


public class UserNotRegisteredException(Guid id)
    : UserException(id, $"User under id: {id} is not registered on this course!");

public class RegisteredAlreadyFinishedException() 
    : UserException(Guid.Empty, $"You can't register on the finished course!");

public class UserAlreadyRegisteredException(Guid id) 
    : UserException(id, $"User under id: {id} is already registered on this course!");

public class UserBannedException(Guid id) 
    : UserException(id, $"User under id: {id} is banned from this course!");

public class UserAlreadyLeftFeedbackException(Guid id) 
    : UserException(id, $"User under id: {id} is already left feedback for this course!");

public class UserWithNameAlreadyExistsException(Guid id) 
    : UserException(id, $"User under such user name already exists!");

public class UserWithEmailAlreadyExistsException(Guid id)
    : UserException(id, $"User under such email already exists!");

public class EmailNotVerifiedException(Guid id)
    : UserException(id, $"User email is not verified!");

public class InvalidVerificationTokenException(Guid id)
    : UserException(id, $"Invalid verification token!");

public class EmailVerificationTokenExpiredException(Guid id)
    : UserException(id, $"Email verification token expired!");


public class InvalidCredentialsException() 
    : UserException(Guid.Empty, $"Invalid credentials!");

public class UserUnauthorizedAccessException(string message) 
    : UserException(Guid.Empty, message);

public class UserUnknownException(Guid id, Exception innerException)
    : UserException(id, $"Unknown exception for the User under id: {id}!", innerException);