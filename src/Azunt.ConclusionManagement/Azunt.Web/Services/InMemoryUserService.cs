using Microsoft.Extensions.Configuration;

namespace Azunt.Web.Services;

/// <summary>
/// Azunt.Web 테스트 프로젝트에서 멀티테넌트 연결 문자열을 흉내 내는 In-Memory UserService입니다.
/// appsettings.json의 ConclusionTenant 섹션을 읽고, 값이 없으면 InMemory:AzuntConclusionTenantDb를 사용합니다.
/// </summary>
public sealed class InMemoryUserService : IUserService
{
    private readonly IConfiguration _configuration;

    public InMemoryUserService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public CurrentUserInfo GetUserNotCached()
    {
        return new CurrentUserInfo
        {
            UserName = _configuration["ConclusionTenant:UserName"] ?? "TenantUser",
            Tenant = new TenantInfo
            {
                Name = _configuration["ConclusionTenant:TenantName"] ?? "In-Memory Tenant",
                ConnectionString = _configuration["ConclusionTenant:ConnectionString"] ?? "InMemory:AzuntConclusionTenantDb"
            }
        };
    }
}
