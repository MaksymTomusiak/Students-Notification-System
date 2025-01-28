using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using Domain.CourseCategories;
using Domain.Courses;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CourseCategoryRepository(ApplicationDbContext context) : ICourseCategoryRepository, ICourseCategoryQueries
{
    public async Task<IReadOnlyList<CourseCategory>> GetAll(CancellationToken cancellationToken)
    {
        return await context.CourseCategories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseCategory>> GetByCourse(CourseId courseId, CancellationToken cancellationToken)
    {
        return await context.CourseCategories
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Option<CourseCategory>> GetById(CourseCategoryId id, CancellationToken cancellationToken)
    {
        var entity = await context.CourseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<CourseCategory>.None: Option<CourseCategory>.Some(entity);
    }

    public async Task<Option<CourseCategory>> GetByCourseAndCategory(CourseId courseId, CategoryId categoryId, CancellationToken cancellationToken)
    {
        var entity = await context.CourseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.CategoryId == categoryId, cancellationToken);
        
        return entity == null ? Option<CourseCategory>.None: Option<CourseCategory>.Some(entity);
    }

    public async Task<CourseCategory> Add(CourseCategory courseCategory, CancellationToken cancellationToken)
    {
        await context.CourseCategories.AddAsync(courseCategory, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseCategory;
    }

    public async Task<CourseCategory> Update(CourseCategory courseCategory, CancellationToken cancellationToken)
    {
        context.CourseCategories.Update(courseCategory);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return courseCategory;
    }

    public async Task<CourseCategory> Delete(CourseCategory courseCategory, CancellationToken cancellationToken)
    {
        context.CourseCategories.Remove(courseCategory);
        
        await context.SaveChangesAsync(cancellationToken);

        return courseCategory;
    }
}