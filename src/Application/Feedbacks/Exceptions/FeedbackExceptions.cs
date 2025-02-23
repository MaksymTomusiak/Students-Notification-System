namespace Application.Feedbacks.Exceptions;

public class FeedbackException(Guid id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public Guid Id { get; } = id;
}

public class FeedbackNotFoundException(Guid id) 
    : FeedbackException(id, $"Feedback under id: {id} not found!");

public class FeedbackUserIdNotFoundException() 
    : FeedbackException(Guid.Empty, $"User id not found!");

public class FeedbackUserNotFoundException() 
    : FeedbackException(Guid.Empty, $"Feedback user not found!");

public class FeedbackCourseNotFoundException() 
    : FeedbackException(Guid.Empty, $"Feedback course not found!");

public class FeedbackAlreadyExistsException(Guid id) 
    : FeedbackException(id, $"Feedback already exists under id: {id}!");

public class FeedbackWithoutRegistrationException(Guid id) 
    : FeedbackException(id, $"You can't leave feedback without registering!");

public class FeedbackUnknownException(Guid id, Exception innerException)
    : FeedbackException(id, $"Unknown exception for the Feedback under id: {id}!", innerException);