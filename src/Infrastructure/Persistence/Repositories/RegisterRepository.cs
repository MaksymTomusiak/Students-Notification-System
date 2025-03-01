using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Courses;
using Domain.Registers;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RegisterRepository(ApplicationDbContext context) : IRegisterRepository, IRegisterQueries
{
    public async Task<IReadOnlyList<Register>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Registers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Register>> GetByCourse(CourseId courseId, CancellationToken cancellationToken)
    {
        return await context.Registers
            .AsNoTracking()
            .Where(x => x.CourseId == courseId)
            .OrderByDescending(x => x.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Register>> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Registers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.RegisteredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Register> Items, int TotalCount, int Page, int PageSize)> GetByUserPaginated(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await context.Registers
            .CountAsync(x => x.UserId == userId, cancellationToken);

        var items = await context.Registers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Course)
            .OrderByDescending(x => x.RegisteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return (items, totalCount, page, pageSize);
    }

    public async Task<Option<Register>> GetById(RegisterId id, CancellationToken cancellationToken)
    {
        var entity = await context.Registers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return entity == null ? Option<Register>.None: Option<Register>.Some(entity);
    }

    public async Task<Option<Register>> GetByCourseAndUser(CourseId courseId, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await context.Registers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == userId, cancellationToken);
        
        return entity == null ? Option<Register>.None: Option<Register>.Some(entity);
    }

    public async Task<Register> Add(Register register, CancellationToken cancellationToken)
    {
        await context.Registers.AddAsync(register, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return register;
    }

    public async Task<Register> Update(Register register, CancellationToken cancellationToken)
    {
        context.Registers.Update(register);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return register;
    }

    public async Task<Register> Delete(Register register, CancellationToken cancellationToken)
    {
        context.Registers.Remove(register);
        
        await context.SaveChangesAsync(cancellationToken);

        return register;
    }
}