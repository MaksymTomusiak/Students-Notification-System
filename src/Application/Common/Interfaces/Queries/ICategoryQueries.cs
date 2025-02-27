using Domain.Categories;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface ICategoryQueries
{
    Task<(IReadOnlyList<Category> Items, int TotalCount, int Page, int PageSize)> GetAllPaginated(
        int page, 
        int pageSize, 
        string? search = null, 
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAll(CancellationToken cancellationToken);
    Task<Option<Category>> GetById(CategoryId id, CancellationToken cancellationToken);
    Task<Option<Category>> SearchByName(string name, CancellationToken cancellationToken);
}