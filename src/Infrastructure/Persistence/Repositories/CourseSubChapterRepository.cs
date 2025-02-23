using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.CourseChapters;
using Domain.CourseSubChapters;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseSubChapterRepository(ApplicationDbContext context) : ICourseSubChapterQueries, ICourseSubChapterRepository
{
    public async Task<IReadOnlyList<CourseSubChapter>> GetAll(CancellationToken cancellationToken)
    {
        return await context.SubChapters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    public async Task<Option<CourseSubChapter>> GetById(CourseSubChapterId id, CancellationToken cancellationToken)
    {
        var entity = await context.SubChapters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<CourseSubChapter>.None: Option<CourseSubChapter>.Some(entity);
    }

    public async Task<IReadOnlyList<CourseSubChapter>> GetByCourseChapterId(CourseChapterId id, CancellationToken cancellationToken)
    {
        return await context.SubChapters
            .AsNoTracking()
            .Where(x => x.CourseChapterId == id)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<CourseSubChapter>> SearchByNameAndChapter(string name, CourseChapterId chapterId, CancellationToken cancellationToken)
    {
        var entity = await context.SubChapters
            .AsNoTracking()
            .Where(x => x.CourseChapterId == chapterId)
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
        
        return entity == null ? Option<CourseSubChapter>.None: Option<CourseSubChapter>.Some(entity);
    }

    public async Task<CourseSubChapter> Add(CourseSubChapter courseSubChapter, CancellationToken cancellationToken)
    {
        await context.SubChapters.AddAsync(courseSubChapter, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseSubChapter;
    }

    public async Task<CourseSubChapter> Update(CourseSubChapter courseSubChapter, CancellationToken cancellationToken)
    {
        context.SubChapters.Update(courseSubChapter);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseSubChapter;
    }

    public async Task<CourseSubChapter> Delete(CourseSubChapter courseSubChapter, CancellationToken cancellationToken)
    {
        context.SubChapters.Remove(courseSubChapter);
        
        await context.SaveChangesAsync(cancellationToken);

        return courseSubChapter;
    }
}