using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Azunt.ConclusionManagement
{
    public class ConclusionRepository : IConclusionRepository
    {
        private readonly ConclusionAppDbContextFactory _factory;
        private readonly ILogger<ConclusionRepository> _logger;

        public ConclusionRepository(
            ConclusionAppDbContextFactory factory,
            ILoggerFactory loggerFactory)
        {
            _factory = factory;
            _logger = loggerFactory.CreateLogger<ConclusionRepository>();
        }

        private ConclusionAppDbContext CreateContext(string? connectionString)
        {
            return string.IsNullOrWhiteSpace(connectionString)
                ? _factory.CreateDbContext()
                : _factory.CreateDbContext(connectionString);
        }

        public async Task<Conclusion> AddAsync(Conclusion model, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            model.Active ??= true;
            model.CreatedAt = model.CreatedAt == default
                ? DateTimeOffset.UtcNow
                : model.CreatedAt;

            context.Conclusions.Add(model);
            await context.SaveChangesAsync();
            return model;
        }

        public async Task<List<Conclusion>> GetAllAsync(string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            return await context.Conclusions
                .OrderByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<Conclusion> GetByIdAsync(long id, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            return await context.Conclusions
                       .SingleOrDefaultAsync(m => m.Id == id)
                   ?? new Conclusion();
        }

        public async Task<bool> UpdateAsync(Conclusion model, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            var entity = await context.Conclusions
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (entity == null)
            {
                return false;
            }

            entity.Active = model.Active;
            entity.Name = model.Name;
            entity.Content = model.Content;
            entity.CreatedAt = model.CreatedAt == default ? entity.CreatedAt : model.CreatedAt;
            entity.CreatedBy = model.CreatedBy;

            context.Conclusions.Update(entity);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(long id, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            var entity = await context.Conclusions
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null)
            {
                return false;
            }

            context.Conclusions.Remove(entity);
            return await context.SaveChangesAsync() > 0;
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
            await using var context = CreateContext(connectionString);

            var query = context.Conclusions.AsQueryable();

            query = ApplySearch(query, searchField, searchQuery);
            query = ApplySort(query, sortOrder);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ArticleSet<Conclusion, int>(items, totalCount);
        }

        public async Task<ArticleSet<Conclusion, long>> GetByAsync<TParentIdentifier>(
            FilterOptions<TParentIdentifier> options,
            string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            var query = context.Conclusions.AsQueryable();

            query = ApplySearch(query, options.SearchField, options.SearchQuery);
            query = ApplySort(query, options.SortOrder);

            var totalCount = await query.LongCountAsync();
            var items = await query
                .Skip(options.PageIndex * options.PageSize)
                .Take(options.PageSize)
                .ToListAsync();

            return new ArticleSet<Conclusion, long>(items, totalCount);
        }

        private static IQueryable<Conclusion> ApplySearch(
            IQueryable<Conclusion> query,
            string? searchField,
            string? searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return query;
            }

            var keyword = searchQuery.Trim();
            var field = searchField?.Trim().ToLowerInvariant();

            return field switch
            {
                "name" => query.Where(m => m.Name != null && m.Name.Contains(keyword)),
                "content" => query.Where(m => m.Content != null && m.Content.Contains(keyword)),
                _ => query.Where(m =>
                    (m.Name != null && m.Name.Contains(keyword)) ||
                    (m.Content != null && m.Content.Contains(keyword)))
            };
        }

        private static IQueryable<Conclusion> ApplySort(IQueryable<Conclusion> query, string? sortOrder)
        {
            return sortOrder switch
            {
                "Name" => query.OrderBy(m => m.Name),
                "NameDesc" => query.OrderByDescending(m => m.Name),
                "Content" => query.OrderBy(m => m.Content),
                "ContentDesc" => query.OrderByDescending(m => m.Content),
                "CreatedAt" => query.OrderBy(m => m.CreatedAt),
                "CreatedAtDesc" => query.OrderByDescending(m => m.CreatedAt),
                "Active" => query.OrderBy(m => m.Active),
                "ActiveDesc" => query.OrderByDescending(m => m.Active),
                _ => query.OrderByDescending(m => m.Id)
            };
        }
    }
}
