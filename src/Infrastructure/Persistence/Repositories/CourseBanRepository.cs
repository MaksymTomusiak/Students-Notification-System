using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using Domain.CourseBans;
using Domain.Courses;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseBanRepository(ApplicationDbContext context) : ICourseBanRepository, ICourseBanQueries
{
    public async Task<IReadOnlyList<CourseBan>> GetAll(CancellationToken cancellationToken)
    {
        return await context.CourseBans
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseBan>> GetByCourse(CourseId courseId, CancellationToken cancellationToken)
    {
        return await context.CourseBans
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseBan>> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await context.CourseBans
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Course)
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<CourseBan>> GetById(CourseBanId id, CancellationToken cancellationToken)
    {
        var entity = await context.CourseBans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<CourseBan>.None: Option<CourseBan>.Some(entity);
    }

    public async Task<Option<CourseBan>> GetByCourseAndUser(CourseId courseId, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await context.CourseBans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == userId, cancellationToken);
        
        return entity == null ? Option<CourseBan>.None: Option<CourseBan>.Some(entity);
    }

    public async Task<CourseBan> Add(CourseBan courseBan, CancellationToken cancellationToken)
    {
        await context.CourseBans.AddAsync(courseBan, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseBan;
    }

    public async Task<CourseBan> Update(CourseBan courseBan, CancellationToken cancellationToken)
    {
        context.CourseBans.Update(courseBan);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseBan;
    }

    public async Task<CourseBan> Delete(CourseBan courseBan, CancellationToken cancellationToken)
    {
        context.CourseBans.Remove(courseBan);
        
        await context.SaveChangesAsync(cancellationToken);

        return courseBan;
    }
}