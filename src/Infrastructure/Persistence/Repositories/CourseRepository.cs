using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using Domain.Courses;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseRepository(ApplicationDbContext context) : ICourseRepository, ICourseQueries
{
    public async Task<IReadOnlyList<Course>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Courses
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(x => x.CreatorId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetByCategories(IReadOnlyList<CategoryId> categories, CancellationToken cancellationToken)
    {
        return await context.Courses
            .AsNoTracking()
            .Include(x => x.CourseCategories)
            .Where(x => x.CourseCategories.Any(cc => categories.Contains(cc.CategoryId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetByDate(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
    {
        return await context.Courses
            .AsNoTracking()
            .Where(x => x.StartDate >= fromDate && x.StartDate <= toDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetCoursesStartingInDays(CancellationToken cancellationToken, params int[] days)
    {
        var upcomingDates = days
            .Select(d => DateTime.UtcNow.Date.AddDays(d))
            .ToList();
        return await context.Courses
            .AsNoTracking()
            .Where(c => upcomingDates.Contains(c.StartDate.Date))
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<Course>> GetById(CourseId id, CancellationToken cancellationToken)
    {
        var entity = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<Course>.None: Option<Course>.Some(entity);
    }

    public async Task<Option<Course>> SearchByName(string name, CancellationToken cancellationToken)
    {
        var entity = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name ==  name, cancellationToken);
        
        return entity == null ? Option<Course>.None : Option<Course>.Some(entity);
    }

    public async Task<Course> Add(Course course, CancellationToken cancellationToken)
    {
        await context.Courses.AddAsync(course, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return course;
    }

    public async Task<Course> Update(Course course, CancellationToken cancellationToken)
    {
        context.Courses.Update(course);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return course;
    }

    public async Task<Course> Delete(Course course, CancellationToken cancellationToken)
    {
        context.Courses.Remove(course);
        
        await context.SaveChangesAsync(cancellationToken);

        return course;
    }
}