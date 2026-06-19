using Azunt.ConclusionManagement;
using Azunt.Web.Services;

namespace Azunt.Web.Components.Pages.Conclusions;

public static class ConclusionTenantSeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IConclusionRepository>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var user = userService.GetUserNotCached();
        var connectionString = user.Tenant.ConnectionString;

        var existing = await repository.GetAllAsync(connectionString);
        if (existing.Count > 0)
        {
            return;
        }

        await repository.AddAsync(new Conclusion
        {
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = user.UserName,
            Name = "Tenant Conclusion 1",
            Content = $"This sample conclusion is stored in the tenant database for {user.Tenant.Name}."
        }, connectionString);

        await repository.AddAsync(new Conclusion
        {
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = user.UserName,
            Name = "Tenant Conclusion 2",
            Content = "This data is loaded through IUserService.GetUserNotCached().Tenant.ConnectionString."
        }, connectionString);
    }
}
