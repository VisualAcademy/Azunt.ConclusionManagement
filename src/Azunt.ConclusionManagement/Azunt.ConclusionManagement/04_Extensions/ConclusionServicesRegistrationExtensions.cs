using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azunt.ConclusionManagement;

/// <summary>
/// ConclusionApp 의존성 주입 확장 메서드
/// </summary>
public static class ConclusionServicesRegistrationExtensions
{
    /// <summary>
    /// ConclusionApp 저장소 모드입니다.
    /// 기본값은 SQL Server 게시 없이 바로 테스트 가능한 EF Core In-Memory입니다.
    /// </summary>
    public enum RepositoryMode
    {
        /// <summary>
        /// SQL Server 게시 없이 Azunt.Web에서 바로 테스트하기 위한 EF Core In-Memory 모드
        /// </summary>
        EfCoreInMemory,

        /// <summary>
        /// SQL Server를 사용하는 EF Core 모드
        /// </summary>
        EfCoreSqlServer,

        /// <summary>
        /// 기존 코드 호환성을 위한 EF Core SQL Server 모드 별칭
        /// </summary>
        EfCore,

        /// <summary>
        /// Dapper 기반 SQL Server 모드
        /// </summary>
        Dapper,

        /// <summary>
        /// ADO.NET 기반 SQL Server 모드
        /// </summary>
        AdoNet
    }

    /// <summary>
    /// ConclusionApp 모듈의 서비스를 등록합니다.
    /// </summary>
    /// <param name="services">서비스 컨테이너</param>
    /// <param name="connectionString">SQL Server 연결 문자열입니다. In-Memory 모드에서는 null이어도 됩니다.</param>
    /// <param name="mode">레포지토리 사용 모드입니다. 기본값은 EF Core In-Memory입니다.</param>
    /// <param name="dbContextLifetime">DbContext 수명 주기입니다. 기본값은 Transient입니다.</param>
    public static void AddDependencyInjectionContainerForConclusionApp(
        this IServiceCollection services,
        string? connectionString = null,
        RepositoryMode mode = RepositoryMode.EfCoreInMemory,
        ServiceLifetime dbContextLifetime = ServiceLifetime.Transient)
    {
        switch (mode)
        {
            case RepositoryMode.EfCoreInMemory:
                services.AddDbContext<ConclusionAppDbContext>(
                    options => options.UseInMemoryDatabase(
                        ConclusionInMemoryDatabase.DefaultName,
                        ConclusionInMemoryDatabase.Root),
                    dbContextLifetime);

                services.AddTransient(_ => new ConclusionAppDbContextFactory());
                services.AddTransient<IConclusionRepository, ConclusionRepository>();
                break;

            case RepositoryMode.EfCoreSqlServer:
            case RepositoryMode.EfCore:
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "SQL Server repository mode requires a valid connection string.");
                }

                services.AddDbContext<ConclusionAppDbContext>(
                    options => options.UseSqlServer(connectionString),
                    dbContextLifetime);

                services.AddTransient(_ => new ConclusionAppDbContextFactory(connectionString));
                services.AddTransient<IConclusionRepository, ConclusionRepository>();
                break;

            case RepositoryMode.Dapper:
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Dapper repository mode requires a valid connection string.");
                }

                services.AddTransient<IConclusionRepository>(provider =>
                    new ConclusionRepositoryDapper(
                        connectionString,
                        provider.GetRequiredService<ILoggerFactory>()));
                break;

            case RepositoryMode.AdoNet:
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "ADO.NET repository mode requires a valid connection string.");
                }

                services.AddTransient<IConclusionRepository>(provider =>
                    new ConclusionRepositoryAdoNet(
                        connectionString,
                        provider.GetRequiredService<ILoggerFactory>()));
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid repository mode '{mode}'. Supported modes: EfCoreInMemory, EfCoreSqlServer, EfCore, Dapper, AdoNet.");
        }
    }
}
