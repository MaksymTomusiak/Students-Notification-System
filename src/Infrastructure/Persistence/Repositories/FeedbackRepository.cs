using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Courses;
using Domain.Feedbacks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FeedbackRepository(ApplicationDbContext context) : IFeedbackRepository, IFeedbackQueries
{
    public async Task<IReadOnlyList<Feedback>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Feedbacks
            .AsNoTracking()
            .Include(x => x.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Feedback> Items, int TotalCount, int Page, int PageSize)> GetByCourse(
        CourseId courseId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        var totalCount = await context.Feedbacks
            .CountAsync(x => x.CourseId == courseId, cancellationToken);

        var items = await context.Feedbacks
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .OrderByDescending(x => x.CreatedAt) // Maintain existing ordering
            .Include(x => x.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount, page, pageSize);
    }
    public async Task<IReadOnlyList<Feedback>> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Feedbacks
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<Feedback>> GetById(FeedbackId id, CancellationToken cancellationToken)
    {
        var entity = await context.Feedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<Feedback>.None: Option<Feedback>.Some(entity);
    }

    public async Task<Option<Feedback>> GetByCourseAndUser(CourseId courseId, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await context.Feedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == userId, cancellationToken);
        
        return entity == null ? Option<Feedback>.None: Option<Feedback>.Some(entity);
    }

    public async Task<Feedback> Add(Feedback feedback, CancellationToken cancellationToken)
    {
        await context.Feedbacks.AddAsync(feedback, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return feedback;
    }

    public async Task<Feedback> Update(Feedback feedback, CancellationToken cancellationToken)
    {
        context.Feedbacks.Update(feedback);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return feedback;
    }

    public async Task<Feedback> Delete(Feedback feedback, CancellationToken cancellationToken)
    {
        context.Feedbacks.Remove(feedback);
        
        await context.SaveChangesAsync(cancellationToken);

        return feedback;
    }
}