using Domain.Feedbacks;

namespace Api.Dtos;

public record FeedbackDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    DateTime CreatedAt,
    string Content,
    ushort Rating)
{
    public static FeedbackDto FromDomainModel(Feedback feedback) => new(
        feedback.Id.Value,
        feedback.UserId,
        feedback.CourseId.Value,
        feedback.CreatedAt,
        feedback.Content,
        feedback.Rating);
}

public record FeedbackCreateDto(
    Guid CourseId,
    string Content,
    ushort Rating);