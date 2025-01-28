﻿using Domain.Courses;
using Domain.Feedbacks;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IFeedbackQueries
{
    Task<IReadOnlyList<Feedback>> GetAll(CancellationToken cancellationToken);
    Task<IReadOnlyList<Feedback>> GetByCourse(CourseId courseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Feedback>> GetByUser(Guid userId, CancellationToken cancellationToken);
    Task<Option<Feedback>> GetById(FeedbackId id, CancellationToken cancellationToken);
    Task<Option<Feedback>> GetByCourseAndUser(CourseId courseId, Guid userId, CancellationToken cancellationToken);
}