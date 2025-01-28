using Domain.Feedbacks;

namespace Application.Common.Interfaces.Repositories;

public interface IFeedbackRepository
{
    Task<Feedback> Add(Feedback feedback, CancellationToken cancellationToken);
    Task<Feedback> Update(Feedback feedback, CancellationToken cancellationToken);
    Task<Feedback> Delete(Feedback feedback, CancellationToken cancellationToken);
}