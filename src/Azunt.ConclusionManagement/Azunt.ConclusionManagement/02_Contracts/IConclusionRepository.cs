
namespace Azunt.ConclusionManagement;

public interface IConclusionRepository
{
    Task<Conclusion> AddAsync(Conclusion model, string? connectionString = null);
    Task<List<Conclusion>> GetAllAsync(string? connectionString = null);
    Task<Conclusion> GetByIdAsync(long id, string? connectionString = null);
    Task<bool> UpdateAsync(Conclusion model, string? connectionString = null);
    Task<bool> DeleteAsync(long id, string? connectionString = null);
    Task<ArticleSet<Conclusion, int>> GetArticlesAsync<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string? connectionString = null);
    Task<ArticleSet<Conclusion, long>> GetByAsync<TParentIdentifier>(FilterOptions<TParentIdentifier> options, string? connectionString = null);
}
