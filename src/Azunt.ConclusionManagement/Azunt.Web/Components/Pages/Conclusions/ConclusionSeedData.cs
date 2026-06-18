using Azunt.ConclusionManagement;

namespace Azunt.Web.Components.Pages.Conclusions;

public static class ConclusionSeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IConclusionRepository>();

        var existing = await repository.GetAllAsync();
        if (existing.Count > 0)
        {
            return;
        }

        await repository.AddAsync(new Conclusion
        {
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "System",
            Name = "Initial Conclusion 1",
            Content = "This is the first sample conclusion for EF Core In-Memory testing."
        });

        await repository.AddAsync(new Conclusion
        {
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "System",
            Name = "Initial Conclusion 2",
            Content = "This is the second sample conclusion for quick Blazor Server CRUD testing."
        });
    }
}
