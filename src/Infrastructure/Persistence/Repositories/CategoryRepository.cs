using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Categories;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository, ICategoryQueries
{
    public async Task<(IReadOnlyList<Category> Items, int TotalCount, int Page, int PageSize)> GetAllPaginated(
        int page, 
        int pageSize, 
        string? search = null, 
        CancellationToken cancellationToken = default)
    {
        var query = context.Categories.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .AsNoTracking()
            .OrderBy(c => c.Name) // Optional: order by name for consistency
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<Category>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Categories.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Option<Category>> GetById(CategoryId id, CancellationToken cancellationToken)
    {
        var entity = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<Category>.None: Option<Category>.Some(entity);
    }

    public async Task<Option<Category>> SearchByName(string name, CancellationToken cancellationToken)
    {
        var entity = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name ==  name, cancellationToken);
        
        return entity == null ? Option<Category>.None : Option<Category>.Some(entity);
    }

    public async Task<Category> Add(Category category, CancellationToken cancellationToken)
    {
        await context.Categories.AddAsync(category, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return category;
    }

    public async Task<Category> Update(Category category, CancellationToken cancellationToken)
    {
        context.Categories.Update(category);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return category;
    }

    public async Task<Category> Delete(Category category, CancellationToken cancellationToken)
    {
        context.Categories.Remove(category);
        
        await context.SaveChangesAsync(cancellationToken);

        return category;
    }
}