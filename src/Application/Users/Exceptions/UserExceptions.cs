﻿namespace Application.Users.Exceptions;

public class UserException(Guid id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public Guid Id { get; } = id;
}

public class UserNotFoundException(Guid id) 
    : UserException(id, $"User under id: {id} not found!");

public class UserIdNotFoundException() 
    : UserException(Guid.Empty, $"User id not found!");

public class RegisteredCourseNotFoundException() 
    : UserException(Guid.Empty, $"Course to register not found!");

public class UserNotRegisteredException(Guid id)
    : UserException(id, $"User under id: {id} is not registered on this course!");

public class RegisteredAlreadyFinishedException() 
    : UserException(Guid.Empty, $"You can't register on the finished course!");

public class UserAlreadyRegisteredException(Guid id) 
    : UserException(id, $"User under id: {id} is already registered on this course!");


public class UserWithNameAlreadyExistsException(Guid id) 
    : UserException(id, $"User under such user name already exists!");

public class UserWithEmailAlreadyExistsException(Guid id)
    : UserException(id, $"User under such email already exists!");

public class InvalidCredentialsException() 
    : UserException(Guid.Empty, $"Invalid credentials!");

public class UserUnauthorizedAccessException(string message) 
    : UserException(Guid.Empty, message);

public class UserUnknownException(Guid id, Exception innerException)
    : UserException(id, $"Unknown exception for the User under id: {id}!", innerException);