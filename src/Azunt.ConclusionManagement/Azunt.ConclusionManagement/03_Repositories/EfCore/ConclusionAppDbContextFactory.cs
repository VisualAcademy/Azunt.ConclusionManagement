using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Azunt.ConclusionManagement;

/// <summary>
/// ConclusionAppDbContext 인스턴스를 생성하는 Factory 클래스입니다.
/// 기본값은 EF Core In-Memory이며, 연결 문자열이 제공되면 SQL Server를 사용합니다.
/// </summary>
public class ConclusionAppDbContextFactory
{
    private readonly IConfiguration? _configuration;
    private readonly string? _defaultConnectionString;

    /// <summary>
    /// 기본 생성자입니다. 연결 문자열 없이 사용하면 In-Memory DbContext를 생성합니다.
    /// </summary>
    public ConclusionAppDbContextFactory()
    {
    }

    /// <summary>
    /// SQL Server 연결 문자열을 직접 전달받는 생성자입니다.
    /// </summary>
    public ConclusionAppDbContextFactory(string defaultConnectionString)
    {
        _defaultConnectionString = defaultConnectionString;
    }

    /// <summary>
    /// IConfiguration을 주입받는 생성자입니다.
    /// DefaultConnection이 있으면 SQL Server, 없으면 In-Memory를 사용합니다.
    /// </summary>
    public ConclusionAppDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 연결 문자열을 사용하여 SQL Server DbContext 인스턴스를 생성합니다.
    /// 빈 문자열이 전달되면 In-Memory DbContext를 생성합니다.
    /// </summary>
    public ConclusionAppDbContext CreateDbContext(string? connectionString)
    {
        return string.IsNullOrWhiteSpace(connectionString)
            ? CreateInMemoryDbContext()
            : CreateSqlServerDbContext(connectionString);
    }

    /// <summary>
    /// DbContextOptions를 사용하여 DbContext 인스턴스를 생성합니다.
    /// </summary>
    public ConclusionAppDbContext CreateDbContext(DbContextOptions<ConclusionAppDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new ConclusionAppDbContext(options);
    }

    /// <summary>
    /// 기본 DbContext 인스턴스를 생성합니다.
    /// 생성자 또는 appsettings.json에 DefaultConnection이 있으면 SQL Server를 사용하고,
    /// 없으면 In-Memory를 사용합니다.
    /// </summary>
    public ConclusionAppDbContext CreateDbContext()
    {
        if (!string.IsNullOrWhiteSpace(_defaultConnectionString))
        {
            return CreateSqlServerDbContext(_defaultConnectionString);
        }

        var configuredConnection = _configuration?.GetConnectionString("DefaultConnection");

        return string.IsNullOrWhiteSpace(configuredConnection)
            ? CreateInMemoryDbContext()
            : CreateSqlServerDbContext(configuredConnection);
    }

    /// <summary>
    /// EF Core In-Memory DbContext 인스턴스를 생성합니다.
    /// </summary>
    public ConclusionAppDbContext CreateInMemoryDbContext(string databaseName = ConclusionInMemoryDatabase.DefaultName)
    {
        var options = new DbContextOptionsBuilder<ConclusionAppDbContext>()
            .UseInMemoryDatabase(databaseName, ConclusionInMemoryDatabase.Root)
            .Options;

        return new ConclusionAppDbContext(options);
    }

    /// <summary>
    /// SQL Server DbContext 인스턴스를 생성합니다.
    /// </summary>
    public ConclusionAppDbContext CreateSqlServerDbContext(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));
        }

        var options = new DbContextOptionsBuilder<ConclusionAppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ConclusionAppDbContext(options);
    }
}
