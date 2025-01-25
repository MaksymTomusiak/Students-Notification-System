namespace Domain.Feedbacks;

public record FeedbackId(Guid Value)
{
    public static FeedbackId Empty() => new(Guid.Empty);
    public static FeedbackId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}