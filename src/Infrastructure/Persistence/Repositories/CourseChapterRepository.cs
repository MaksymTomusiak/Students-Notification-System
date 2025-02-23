using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.CourseChapters;
using Domain.Courses;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseChapterRepository(ApplicationDbContext context) : ICourseChapterQueries, ICourseChapterRepository
{
    public async Task<IReadOnlyList<CourseChapter>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Chapters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    public async Task<Option<CourseChapter>> GetById(CourseChapterId id, CancellationToken cancellationToken)
    {
        var entity = await context.Chapters
            .AsNoTracking()
            .Include(x => x.SubChapters)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<CourseChapter>.None: Option<CourseChapter>.Some(entity);
    }

    public async Task<IReadOnlyList<CourseChapter>> GetByCourseId(CourseId id, CancellationToken cancellationToken)
    {
        return await context.Chapters
            .AsNoTracking()
            .Where(x => x.CourseId == id)
            .Include(x => x.SubChapters.OrderBy(subChapter => subChapter.Number))
            .OrderBy(x => x.Number)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<CourseChapter>> SearchByNameAndCourse(string name, CourseId courseId, CancellationToken cancellationToken)
    {
        var entity = await context.Chapters
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
        
        return entity == null ? Option<CourseChapter>.None: Option<CourseChapter>.Some(entity);
    }

    public async Task<CourseChapter> Add(CourseChapter courseChapter, CancellationToken cancellationToken)
    {
        await context.Chapters.AddAsync(courseChapter, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseChapter;
    }

    public async Task<CourseChapter> Update(CourseChapter courseChapter, CancellationToken cancellationToken)
    {
        context.Chapters.Update(courseChapter);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseChapter;
    }

    public async Task<CourseChapter> Delete(CourseChapter courseChapter, CancellationToken cancellationToken)
    {
        context.Chapters.Remove(courseChapter);
        
        await context.SaveChangesAsync(cancellationToken);

        return courseChapter;
    }
}