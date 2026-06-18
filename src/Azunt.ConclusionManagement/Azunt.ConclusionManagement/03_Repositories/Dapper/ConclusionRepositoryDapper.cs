using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Azunt.ConclusionManagement;

public class ConclusionRepositoryDapper : IConclusionRepository
{
    private readonly string _defaultConnectionString;
    private readonly ILogger<ConclusionRepositoryDapper> _logger;

    public ConclusionRepositoryDapper(string defaultConnectionString, ILoggerFactory loggerFactory)
    {
        _defaultConnectionString = defaultConnectionString;
        _logger = loggerFactory.CreateLogger<ConclusionRepositoryDapper>();
    }

    private SqlConnection GetConnection(string? connectionString)
    {
        return new SqlConnection(connectionString ?? _defaultConnectionString);
    }

    public async Task<Conclusion> AddAsync(Conclusion model, string? connectionString = null)
    {
        await using var conn = GetConnection(connectionString);

        var sql = @"INSERT INTO Conclusions (Active, CreatedAt, CreatedBy, Name, Content)
                    OUTPUT INSERTED.Id
                    VALUES (@Active, @CreatedAt, @CreatedBy, @Name, @Content)";

        model.Active ??= true;
        model.CreatedAt = model.CreatedAt == default ? DateTimeOffset.UtcNow : model.CreatedAt;
        model.Id = await conn.ExecuteScalarAsync<long>(sql, model);
        return model;
    }

    public async Task<List<Conclusion>> GetAllAsync(string? connectionString = null)
    {
        await using var conn = GetConnection(connectionString);

        var sql = "SELECT Id, Active, CreatedAt, CreatedBy, Name, Content FROM Conclusions ORDER BY Id DESC";
        var list = await conn.QueryAsync<Conclusion>(sql);
        return list.ToList();
    }

    public async Task<Conclusion> GetByIdAsync(long id, string? connectionString = null)
    {
        await using var conn = GetConnection(connectionString);

        var sql = "SELECT Id, Active, CreatedAt, CreatedBy, Name, Content FROM Conclusions WHERE Id = @Id";
        var model = await conn.QuerySingleOrDefaultAsync<Conclusion>(sql, new { Id = id });
        return model ?? new Conclusion();
    }

    public async Task<bool> UpdateAsync(Conclusion model, string? connectionString = null)
    {
        await using var conn = GetConnection(connectionString);

        var sql = @"UPDATE Conclusions SET
                        Active = @Active,
                        Name = @Name,
                        Content = @Content
                    WHERE Id = @Id";

        var rows = await conn.ExecuteAsync(sql, model);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long id, string? connectionString = null)
    {
        await using var conn = GetConnection(connectionString);

        var sql = "DELETE FROM Conclusions WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<ArticleSet<Conclusion, int>> GetArticlesAsync<TParentIdentifier>(
        int pageIndex,
        int pageSize,
        string searchField,
        string searchQuery,
        string sortOrder,
        TParentIdentifier parentIdentifier,
        string? connectionString = null)
    {
        var all = await GetAllAsync(connectionString);
        var filtered = ApplySearch(all, searchField, searchQuery);
        var sorted = ApplySort(filtered, sortOrder);

        var paged = sorted
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return new ArticleSet<Conclusion, int>(paged, filtered.Count);
    }

    public async Task<ArticleSet<Conclusion, long>> GetByAsync<TParentIdentifier>(
        FilterOptions<TParentIdentifier> options,
        string? connectionString = null)
    {
        var all = await GetAllAsync(connectionString);
        var filtered = ApplySearch(all, options.SearchField, options.SearchQuery);
        var sorted = ApplySort(filtered, options.SortOrder);

        var paged = sorted
            .Skip(options.PageIndex * options.PageSize)
            .Take(options.PageSize)
            .ToList();

        return new ArticleSet<Conclusion, long>(paged, filtered.Count);
    }

    private static List<Conclusion> ApplySearch(IEnumerable<Conclusion> source, string? searchField, string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return source.ToList();
        }

        var keyword = searchQuery.Trim();
        var field = searchField?.Trim().ToLowerInvariant();

        return field switch
        {
            "name" => source.Where(m => m.Name != null && m.Name.Contains(keyword)).ToList(),
            "content" => source.Where(m => m.Content != null && m.Content.Contains(keyword)).ToList(),
            _ => source.Where(m =>
                (m.Name != null && m.Name.Contains(keyword)) ||
                (m.Content != null && m.Content.Contains(keyword))).ToList()
        };
    }

    private static IEnumerable<Conclusion> ApplySort(IEnumerable<Conclusion> source, string? sortOrder)
    {
        return sortOrder switch
        {
            "Name" => source.OrderBy(m => m.Name),
            "NameDesc" => source.OrderByDescending(m => m.Name),
            "Content" => source.OrderBy(m => m.Content),
            "ContentDesc" => source.OrderByDescending(m => m.Content),
            "CreatedAt" => source.OrderBy(m => m.CreatedAt),
            "CreatedAtDesc" => source.OrderByDescending(m => m.CreatedAt),
            "Active" => source.OrderBy(m => m.Active),
            "ActiveDesc" => source.OrderByDescending(m => m.Active),
            _ => source.OrderByDescending(m => m.Id)
        };
    }
}
