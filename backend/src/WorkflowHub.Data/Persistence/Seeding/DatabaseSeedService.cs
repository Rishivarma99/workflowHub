namespace WorkflowHub.Data.Persistence.Seeding;

public sealed class DatabaseSeedService(LearnContentSeedService learnContentSeedService)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await learnContentSeedService.SeedAsync(cancellationToken);
    }
}
