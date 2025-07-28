using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace JustAnAiAgent.Data;

public class JustAnAiAgentDbContextFactory : IDesignTimeDbContextFactory<JustAnAiAgentDbContext>
{
    public JustAnAiAgentDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("JustAnAiAgent");

        var optionsBuilder = new DbContextOptionsBuilder<JustAnAiAgentDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new JustAnAiAgentDbContext(optionsBuilder.Options);
    }
}